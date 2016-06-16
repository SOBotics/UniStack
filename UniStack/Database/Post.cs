﻿using System.Collections.Generic;
using Npgsql;

namespace UniStack.Database
{
    public class Post
    {
        private NpgsqlConnectionStringBuilder conStr;
        private float? length;

        public int PostID { get; set; }

        public float Length
        {
            get
            {
                return length ?? 0;
            }
            set
            {
                if (conStr == null || length == null)
                {
                    length = value;
                    return;
                }

                length = value;

                var cmdStr = $"UPDATE posts SET length = {value} WHERE postid = {PostID};";

                using (var con = new NpgsqlConnection(conStr))
                using (var cmd = new NpgsqlCommand(cmdStr, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public string Tags { get; set; }

        public IEnumerable<LocalTerm> LTerms
        {
            get
            {
                if (conStr == null) yield break;

                // No need to parameterise this.
                var cmdStr = $"SELECT * FROM localterms WHERE postid = {PostID};";

                using (var con = new NpgsqlConnection(conStr))
                using (var cmd = new NpgsqlCommand(cmdStr, con))
                {
                    con.Open();

                    var reader = cmd.ExecuteReader();

                    foreach (var entry in reader)
                    {
                        yield return new LocalTerm(conStr)
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
        }



        public Post(NpgsqlConnectionStringBuilder connection = null)
        {
            conStr = connection;
        }
    }
}
