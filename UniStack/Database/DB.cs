using System.Collections.Generic;
using System.Text;
using Npgsql;

namespace UniStack.Database
{
    public class DB
    {
        private readonly NpgsqlConnectionStringBuilder conStrBuilder;



        public DB(NpgsqlConnectionStringBuilder connection, bool dbExists = false)
        {
            conStrBuilder = connection;

            if (!dbExists)
            {
                var sucess = CreateDB();

                if (sucess)
                {
                    CreateTables();
                }
            }
        }



        public long GetPostCount()
        {
            var createTables = $"SELECT count(*) FROM posts;";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(createTables, con))
            {
                con.Open();
                return cmd.ExecuteScalar() as long? ?? 0;
            }
        }

        #region Record existence checking methods.

        public bool GlobalTermExists(int value)
        {
            var createTables = $"SELECT EXISTS (SELECT 1 FROM globalterms WHERE value = {value});";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(createTables, con))
            {
                con.Open();
                var t = cmd.ExecuteScalar();
                return t as bool? ?? false;
            }
        }

        public bool PostExists(int postID)
        {
            var createTables = $"SELECT EXISTS (SELECT 1 from posts WHERE postid = {postID});";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(createTables, con))
            {
                con.Open();
                return cmd.ExecuteScalar() as bool? ?? false;
            }
        }

        #endregion

        #region Record adding methods.

        public void AddGlobalTerm(GlobalTerm gterm)
        {
            var cmdStr = $"INSERT INTO globalterms VALUES (@value, @idf);";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr, con))
            {
                cmd.Parameters.AddWithValue("value", gterm.Value);
                cmd.Parameters.AddWithValue("idf", gterm.Idf);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AddGlobalTerms(IEnumerable<GlobalTerm> gterms)
        {
            var cmdStr = new StringBuilder("INSERT INTO globalterms VALUES");
            var i = 0;

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr.ToString(), con))
            {
                foreach (var term in gterms)
                {
                    cmdStr.Append($" (@value{i}, @idf{i}),");

                    cmd.Parameters.AddWithValue($"value{i}", term.Value);
                    cmd.Parameters.AddWithValue($"idf{i}", term.Idf);
                    i++;
                }

                cmdStr.Length -= 1; // Remove extra comma.
                cmdStr.Append(";");

                cmd.CommandText = cmdStr.ToString();

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AddLocalTerm(LocalTerm lterm)
        {
            var cmdStr = "INSERT INTO localterms (postid, value, tf, vector) VALUES (@postid, @value, @tf, @vector);";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr, con))
            {
                cmd.Parameters.AddWithValue("postid", lterm.PostID);
                cmd.Parameters.AddWithValue("value", lterm.Value);
                cmd.Parameters.AddWithValue("tf", lterm.TF);
                cmd.Parameters.AddWithValue("vector", lterm.Vector);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AddLocalTerms(IEnumerable<LocalTerm> lterms)
        {
            var cmdStr = new StringBuilder("INSERT INTO localterms (postid, value, tf, vector) VALUES");
            var i = 0;

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr.ToString(), con))
            {
                foreach (var term in lterms)
                {
                    cmdStr.Append($" (@postid{i}, @value{i}, @tf{i}, @vector{i}),");

                    cmd.Parameters.AddWithValue($"postid{i}", term.PostID);
                    cmd.Parameters.AddWithValue($"value{i}", term.Value);
                    cmd.Parameters.AddWithValue($"tf{i}", term.TF);
                    cmd.Parameters.AddWithValue($"vector{i}", term.Vector);
                    i++;
                }

                cmdStr.Length -= 1; // Remove extra comma.
                cmdStr.Append(";");

                cmd.CommandText = cmdStr.ToString();

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void AddPost(Post post)
        {
            var cmdStr = "INSERT INTO posts VALUES (@postid, @length, @tags);";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr, con))
            {
                cmd.Parameters.AddWithValue("postid", post.PostID);
                cmd.Parameters.AddWithValue("length", post.Length);
                cmd.Parameters.AddWithValue("tags", NpgsqlTypes.NpgsqlDbType.Varchar, post.Tags);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Record fetching methods.

        public IEnumerable<GlobalTerm> GetGlobalTerms()
        {
            var cmdStr = "SELECT * FROM globalterms;";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr, con))
            {
                con.Open();

                var reader = cmd.ExecuteReader();

                foreach (var entry in reader)
                {
                    yield return new GlobalTerm(conStrBuilder)
                    {
                        Idf = (float)reader["idf"],
                        Value = (int)reader["value"]
                    };
                }
            }
        }

        public GlobalTerm GetGlobalTerm(int value)
        {
            // No need to parameterise this.
            var cmdStr = $"SELECT * FROM globalterms WHERE value = {value};";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr, con))
            {
                con.Open();

                var reader = cmd.ExecuteReader(System.Data.CommandBehavior.SingleRow);

                var gt = new GlobalTerm(conStrBuilder);

                foreach (var row in reader)
                {
                    gt.Idf = (float)reader["idf"];
                    gt.Value = (int)reader["value"];
                    break;
                }

                return gt;
            }
        }

        public IEnumerable<LocalTerm> GetLocalTerms()
        {
            var cmdStr = "SELECT * FROM localterms;";

            cmdStr += ";";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr, con))
            {
                con.Open();

                var reader = cmd.ExecuteReader();

                foreach (var entry in reader)
                {
                    yield return new LocalTerm(conStrBuilder)
                    {
                        ID = (long)reader["id"],
                        PostID = (int)reader["postid"],
                        TF = (short)reader["tf"],
                        Value = (int)reader["value"],
                        Vector = (float)reader["vector"]
                    };
                }
            }
        }

        public IEnumerable<Post> GetPosts()
        {
            var cmdStr = "SELECT * FROM posts;";

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(cmdStr, con))
            {
                con.Open();

                var reader = cmd.ExecuteReader();

                foreach (var entry in reader)
                {
                    yield return new Post(conStrBuilder)
                    {
                        PostID = (int)reader["postid"],
                        Length = (float)reader["length"],
                        Tags = (string)reader["tags"]
                    };
                }
            }
        }

        public IEnumerable<Post> GetPostsContainingLocalTerms(IEnumerable<int> termValues)
        {
            var getPosts = new StringBuilder(@"CREATE TEMP TABLE temp AS
                                               SELECT posts.postid, posts.length, posts.tags FROM posts
                                               INNER JOIN localterms ON posts.postid = localterms.postid
                                               WHERE localterms.value IN (");

            foreach (var termHash in termValues)
            {
                getPosts.Append(termHash);
                getPosts.Append(",");
            }

            getPosts.Length -= 1;
            getPosts.Append("); SELECT DISTINCT * FROM temp;");

            using (var con = new NpgsqlConnection(conStrBuilder))
            using (var cmd = new NpgsqlCommand(getPosts.ToString(), con))
            {
                con.Open();

                var reader = cmd.ExecuteReader();

                foreach (var entry in reader)
                {
                    yield return new Post(conStrBuilder)
                    {
                        PostID = (int)reader["postid"],
                        Length = (float)reader["length"],
                        Tags = (string)reader["tags"]
                    };
                }
            }
        }

        #endregion

        public bool DeleteDB()
        {
            var createTables = $"DROP DATABASE \"{conStrBuilder.Database}\";";

            var dbName = conStrBuilder.Database;
            conStrBuilder.Database = null;

            try
            {
                using (var con = new NpgsqlConnection(conStrBuilder))
                using (var cmd = new NpgsqlCommand(createTables, con))
                {
                    con.Open();
                    var res = cmd.ExecuteScalar();
                    return res == null;
                }
            }
            catch // Something bad happened, that's all we need to know.
            {
                return false;
            }
            finally
            {
                conStrBuilder.Database = dbName;
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
                    idf   float4
                );
                CREATE TABLE IF NOT EXISTS posts
                (
                    postid int4         PRIMARY KEY,
                    length float4,
                    tags   varchar(128) NOT NULL
                );
                CREATE TABLE IF NOT EXISTS localterms
                (
                    id     bigserial PRIMARY KEY,
                    postid int4      REFERENCES Posts(PostID),
                    value  int4      REFERENCES GlobalTerms(Value),
                    tf     int2,
                    vector float4
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

/*
 postid | length  | value  |  vector
--------+---------+--------+----------
      9 | 19.5836 |    864 | 0.187707
     39 | 74.8015 |    864 | 0.750829
     39 | 74.8015 |  28357 |  1.13924
    109 | 95.3455 |    864 |  3.56644
    109 | 95.3455 |  28357 |  2.27847
    260 | 63.5686 |    864 |  1.68936
   1037 | 36.2826 |    864 | 0.375414
   1010 | 45.9522 |  28357 |  1.13924
   1040 | 34.8206 |  28357 |  2.27847
   1528 | 58.2363 |    864 |  1.68936
   1528 | 58.2363 |  28357 |  3.41771
   1760 | 41.7471 |  28357 |  2.27847
   1848 | 53.5024 |    864 |  1.87707
   1836 | 49.7332 |  28357 |  1.13924
   1848 | 53.5024 |  28357 |  2.27847
   1898 | 24.4472 |  28357 |  1.13924
   1994 | 25.8593 |    864 | 0.187707
   2267 | 60.5401 |    864 | 0.750829
   2267 | 60.5401 |  28357 |  1.13924
   2871 | 54.1987 |  28357 |  1.13924
   2872 | 169.377 |  28357 |  2.27847
   2872 | 169.377 | 949793 |  3.53952
 */
