using System;
using System.Collections.Generic;
using System.Text;
using Npgsql;

// Special thanks to Tunaki (http://stackoverflow.com/users/1743880)
// for optimising most of these queries.

namespace UniStack
{
    /// <summary>
    /// Term weighting: TF-IDF.
    /// Similarity function: cosine.
    /// 
    /// This object is NOT thread-safe.
    /// </summary>
    public class BagOfWordsDB
    {
        private readonly NpgsqlConnectionStringBuilder conStrBuilder;
        private bool minipulatedSinceLastRecalc;



        public BagOfWordsDB(NpgsqlConnectionStringBuilder connectionStringBuilder)
        {
            conStrBuilder = connectionStringBuilder;

            var sucess = CreateDB();

            if (sucess)
            {
                CreateTables();
            }
        }



        public bool ContainsPost(int postID)
        {
            var checkPosts = $"SELECT 1 FROM posts WHERE postid = {postID};";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(checkPosts, con))
            {
                con.Open();

                return cmd.ExecuteScalar() != null;
            }
        }

        public void AddPost(PostDB post)
        {
            if (post == null) throw new ArgumentNullException(nameof(post));
            if (post.Terms == null) throw new ArgumentNullException(nameof(post.Terms));
            if (ContainsPost(post.ID))
            {
                throw new ArgumentException("A post with this ID already exists.", nameof(post));
            }

            minipulatedSinceLastRecalc = true;

            var addPost = $"INSERT INTO posts (postid, tags) VALUES ({post.ID}, @postTags);";

            var addGlobalTerms = $@"INSERT INTO globalterms (value) VALUES <queryTermHashes>
                                    ON CONFLICT ON CONSTRAINT globalterms_pkey DO NOTHING;";

            var gTermHashes = new StringBuilder();

            foreach (var term in post.Terms)
            {
                gTermHashes.Append("(");
                gTermHashes.Append(term.Key);
                gTermHashes.Append("),");
            }

            gTermHashes.Length -= 1;

            addGlobalTerms = addGlobalTerms.Replace("<queryTermHashes>", gTermHashes.ToString());

            var addLocalTerms = "INSERT INTO localterms (postid, value, tf) VALUES <queryLocalTerms>;";

            var lTermHashes = new StringBuilder();

            foreach (var term in post.Terms)
            {
                lTermHashes.Append("(");
                lTermHashes.Append(post.ID);
                lTermHashes.Append(",");
                lTermHashes.Append(term.Key);
                lTermHashes.Append(",");
                lTermHashes.Append(term.Value);
                lTermHashes.Append("),");
            }

            lTermHashes.Length -= 1;

            addLocalTerms = addLocalTerms.Replace("<queryLocalTerms>", lTermHashes.ToString());

            using (var con = new NpgsqlConnection(conStrBuilder))
            {
                con.Open();

                using (var cmd = new NpgsqlCommand(addPost, con))
                {
                    cmd.Parameters.AddWithValue("postTags", NpgsqlTypes.NpgsqlDbType.Varchar, post.Tags);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand(addGlobalTerms, con))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand(addLocalTerms, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void RemovePost(int postID)
        {
            //TODO: Upgrade this to something "better". Not sure what yet...

            //if (!ContainsPost(postID))
            //{
            //    throw new KeyNotFoundException("Cannot find any posts with the specified ID.");
            //}

            //minipulatedSinceLastRecalc = true;

            //var db = new DB(conStrBuilder, true);

            //var p = db.Posts.Single(x => x.PostID == postID);

            //foreach (var t in p.Terms)
            //{
            //    db.LocalTerms.Remove(t);
            //}

            //db.Posts.Remove(p);

            throw new NotImplementedException();
        }

        public Dictionary<int, float> GetSimilarity(IDictionary<int, short> queryTermHashesByCount, string postTopTag, uint maxPostsToReturn = uint.MaxValue)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateData();
                minipulatedSinceLastRecalc = false;
            }

            var queryLen = -1F;
            var queryVectors = GetQueryVectors(queryTermHashesByCount, out queryLen);

            var sql = new StringBuilder(@"CREATE TEMP TABLE queryterms
                                          (
                                              value  int4   PRIMARY KEY,
                                              vector float4
                                          );
                                          INSERT INTO queryterms VALUES ");

            foreach (var term in queryVectors)
            {
                sql.Append("(");
                sql.Append(term.Key);
                sql.Append(",");
                sql.Append(term.Value);
                sql.Append("),");
            }

            sql.Length -= 1;
            sql.Append(";");

            sql.Append($@"CREATE TEMP TABLE tempposts AS
                          SELECT posts.postid, posts.length, localterms.vector AS lterm, queryterms.vector AS qterm
                          FROM posts
                          INNER JOIN localterms ON posts.postid = localterms.postid
                          INNER JOIN queryterms ON localterms.value = queryterms.value
                          WHERE tags LIKE @queryTag
                          AND localterms.value IN (");

            foreach (var termHash in queryTermHashesByCount.Keys)
            {
                sql.Append(termHash);
                sql.Append(",");
            }

            sql.Length -= 1;
            sql.Append(");");

            sql.Append($@"SELECT postid, sum(lterm * qterm) / (length * {queryLen}) AS sim
                          FROM tempposts
                          GROUP BY postid, length
                          ORDER BY sim DESC
                          LIMIT {maxPostsToReturn};");

            var simResults = new Dictionary<int, float>();
            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(sql.ToString(), con))
            {
                con.Open();

                cmd.Parameters.AddWithValue("queryTag", postTopTag + "%");

                var reader = cmd.ExecuteReader();

                foreach (var entry in reader)
                {
                    var postID = (int)reader["postid"];
                    var sim = (double)reader["sim"];

                    simResults[postID] = (float)sim;
                }
            }

            return simResults;
        }



        private Dictionary<int, float> GetQueryVectors(IDictionary<int, short> queryTermHashesByCount, out float queryLength)
        {

            var getQueryVectors = @"CREATE TEMP TABLE queryterms
                                    (
                                        value int4 PRIMARY KEY,
                                        count int2,
                                        idf   float4,
                                        max   int2
                                    );
                                    INSERT INTO queryterms (value, count) VALUES <queryTerms>;

                                    UPDATE queryterms
                                    SET idf = globalterms.idf, max = (SELECT max(count) FROM queryterms)
                                    FROM globalterms
                                    WHERE queryterms.value = globalterms.value;

                                    SELECT value, idf * (count * 1.0 / max) As vector
                                    FROM queryterms;";

            var queryTermsStr = new StringBuilder();

            foreach (var term in queryTermHashesByCount)
            {
                queryTermsStr.Append("(");
                queryTermsStr.Append(term.Key);
                queryTermsStr.Append(",");
                queryTermsStr.Append(term.Value);
                queryTermsStr.Append("),");
            }

            queryTermsStr.Length -= 1;

            getQueryVectors = getQueryVectors.Replace("<queryTerms>", queryTermsStr.ToString());

            var queryVectors = new Dictionary<int, float>();
            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(getQueryVectors, con))
            {
                con.Open();

                var reader = cmd.ExecuteReader();

                foreach (var entry in reader)
                {
                    var value = (int)reader["value"];
                    var vector = reader["vector"] as double? ?? 0;

                    queryVectors[value] = (float)vector;
                }
            }

            // Calculate the query's Euclidean length.
            var queryLen = 0F;
            foreach (var vector in queryVectors.Values)
            {
                queryLen += vector * vector;
            }
            queryLength = (float)Math.Sqrt(queryLen);

            return queryVectors;
        }

        private void RecalculateData()
        {
            var getPostCount = "SELECT count(*) FROM posts;";

            var updateIdfs = @"WITH idfs AS
                               (
                                   SELECT value, log(2, 1000.0 / count(*)) AS idf
                                   FROM localterms
                                   GROUP BY value
                               )
                               UPDATE globalterms
                               SET idf = idfs.idf
                               FROM idfs
                               WHERE globalterms.value = idfs.value;";

            var updateVectors = @"UPDATE localterms SET vector = tf * idf
                                  FROM globalterms 
                                  WHERE globalterms.value = localterms.value;";

            var updateLength = @"WITH new_length(postid, length) AS
                                 (
                                     SELECT postid, |/ sum(vector * vector)
                                     FROM localterms
                                     GROUP BY postid
                                 )
                                 UPDATE posts SET length = nl.length
                                 FROM new_length nl
                                 WHERE posts.postid = nl.postid;";

            using (var con = new NpgsqlConnection(conStrBuilder))
            {
                con.Open();

                var postCount = -1L;

                using (var cmd = new NpgsqlCommand(getPostCount, con))
                {
                    postCount = cmd.ExecuteScalar() as long? ?? -1;
                }

                updateIdfs = updateIdfs.Replace("<postCount>", postCount.ToString());

                using (var cmd = new NpgsqlCommand(updateIdfs, con))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand(updateVectors, con))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new NpgsqlCommand(updateLength, con))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private bool CreateDB()
        {
            var createDB = $@"
                CREATE DATABASE ""{conStrBuilder.Database}""
                WITH OWNER = ""{conStrBuilder.Username}""
                ENCODING = 'UTF8'
                CONNECTION LIMIT = -1;";

            var dbName = conStrBuilder.Database;
            conStrBuilder.Database = null;

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(createDB, con))
            {
                con.Open();

                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch (PostgresException ex) when (ex.SqlState == "42P04")
                {
                    // The DB already exists, continue.

                    return false;
                }
                finally
                {
                    conStrBuilder.Database = dbName;
                }
            }

            return true;
        }

        private void CreateTables()
        {
            var createTables = $@"
                CREATE TABLE IF NOT EXISTS globalterms
                (
                    value int4   PRIMARY KEY,
                    idf   float4 DEFAULT 0
                );
                CREATE TABLE IF NOT EXISTS posts
                (
                    postid int4         PRIMARY KEY,
                    length float4       DEFAULT 0,
                    tags   varchar(128) NOT NULL
                );
                CREATE TABLE IF NOT EXISTS localterms
                (
                    id     bigserial PRIMARY KEY,
                    postid int4      REFERENCES Posts(PostID),
                    value  int4      REFERENCES GlobalTerms(Value),
                    tf     int2      DEFAULT 0,
                    vector float4    DEFAULT 0
                );";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(createTables, con))
            {
                con.Open();
                var res = cmd.ExecuteNonQuery();
            }
        }
    }
}
