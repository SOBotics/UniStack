using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using UniStack.Database;

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

            var db = new DB(conStrBuilder);
        }



        public bool ContainsPost(int postID)
        {
            var db = new DB(conStrBuilder, true);

            return db.PostExists(postID);
        }

        public void AddPost(int postID, string tags, IDictionary<int, short> termHashesByCount)
        {
            //TODO: Upgrade this to something "better". Not sure what yet...

            //if (termHashesByCount == null) throw new ArgumentNullException(nameof(termHashesByCount));
            //if (ContainsPost(postID))
            //{
            //    throw new ArgumentException("A post with this ID already exists.", nameof(postID));
            //}

            //minipulatedSinceLastRecalc = true;

            //var db = new DB(conStrBuilder, true);

            //db.AddPost(new Database.Post
            //{
            //    PostID = postID,
            //    Tags = tags
            //});

            //foreach (var t in termHashesByCount)
            //{
            //    if (!db.GlobalTermExists(t.Key))
            //    {
            //        db.AddGlobalTerm(new GlobalTerm
            //        {
            //            Value = t.Key
            //        });
            //    }

            //    db.AddLocalTerm(new LocalTerm(conStrBuilder)
            //    {
            //        PostID = postID,
            //        Value = t.Key,
            //        TF = t.Value
            //    });
            //}

            throw new NotImplementedException();
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

        public Dictionary<int, float> GetSimilarity(IDictionary<int, short> queryTermHashesByCount, string postTopTag, uint maxPostsToReturn = uint.MaxValue, double minSimilarity = 0)
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

            sql.Append(@"CREATE TEMP TABLE tempposts AS
                         SELECT posts.postid, posts.length, localterms.vector AS lterm, queryterms.vector AS qterm FROM posts
                         INNER JOIN localterms ON posts.postid = localterms.postid
                         INNER JOIN queryterms ON localterms.value = queryterms.value
                         WHERE localterms.value IN (");

            foreach (var termHash in queryTermHashesByCount.Keys)
            {
                sql.Append(termHash);
                sql.Append(",");
            }

            sql.Length -= 1;
            sql.Append(");");

            sql.Append($@"SELECT postid, SUM(lterm * qterm) / (length * {queryLen}) AS sim
                          FROM tempposts
                          GROUP BY postid, length
                          ORDER BY sim DESC
                          LIMIT {maxPostsToReturn};");

            var simResults = new Dictionary<int, float>();
            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(sql.ToString(), con))
            {
                con.Open();

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
            var db = new DB(conStrBuilder, true);

            // Get the count of the most common term in the query.
            var maxQueryTermCount = 0F;
            foreach (var qt in queryTermHashesByCount)
            {
                if (qt.Value > maxQueryTermCount)
                {
                    maxQueryTermCount = qt.Value;
                }
            }

            // Generate the query's TF-IDF values (vectors).
            var queryVectors = new Dictionary<int, float>();
            foreach (var qt in queryTermHashesByCount)
            {
                if (db.GlobalTermExists(qt.Key))
                {
                    var termIdf = db.GetGlobalTerm(qt.Key).Idf;
                    queryVectors[qt.Key] = termIdf * (qt.Value / maxQueryTermCount);
                }
            }

            // Calculate the query's Euclidean length.
            var queryLen = 0F;
            foreach (var tfIdf in queryVectors.Values)
            {
                queryLen += tfIdf * tfIdf;
            }
            queryLength = (float)Math.Sqrt(queryLen);

            return queryVectors;
        }

        private void RecalculateData()
        {
            var getPostCount = "SELECT count(*) FROM posts;";

            var updateIdfs = @"UPDATE globalterms
                               SET idf = 
                               (
                                   SELECT log(2, <postCount>.0 / count(*))
                                   FROM localterms
                                   WHERE localterms.value = globalterms.value
                                   GROUP BY value
                               );";

            var updateVectors = $@"CREATE TEMP TABLE tempterms AS
                                   SELECT localterms.value, postid, tf * idf AS vector
                                   FROM localterms
                                   INNER JOIN globalterms on localterms.value = globalterms.value;

                                   UPDATE localterms
                                   SET vector = tempterms.vector
                                   FROM tempterms
                                   WHERE localterms.value = tempterms.value 
                                   AND localterms.postid = tempterms.postid;";

            var updateLength = @"UPDATE posts
                                 SET length =
                                 (
                                     SELECT |/ sum(vector * vector)
                                     FROM localterms
                                     WHERE localterms.postid = posts.postid
                                 );";

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
    }
}
