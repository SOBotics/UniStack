using System.Collections.Generic;
using Npgsql;

namespace UniStack.Database
{
    public class GlobalTerm
    {
        private NpgsqlConnectionStringBuilder conStr;
        private float? idf;

        public int Value { get; set; }

        public float Idf
        {
            get
            {
                return idf ?? 0;
            }
            set
            {
                if (conStr == null || idf == null)
                {
                    idf = value;
                    return;
                }

                idf = value;

                var cmdStr = $"UPDATE globalterms SET idf = {value} WHERE value = {Value};";

                using (var con = new NpgsqlConnection(conStr))
                using (var cmd = new NpgsqlCommand(cmdStr, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<LocalTerm> LTerms
        {
            get
            {
                if (conStr == null) yield break;

                var cmdStr = $"SELECT * FROM localterms WHERE value = {Value};";

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



        public GlobalTerm(NpgsqlConnectionStringBuilder connection = null)
        {
            conStr = connection;
        }
    }
}
