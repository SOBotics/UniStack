using Npgsql;

namespace UniStack.Database
{
    public class LocalTerm
    {
        private NpgsqlConnectionStringBuilder conStr;
        private float? vector;

        public long ID { get; set; }

        public int PostID { get; set; }

        public int Value { get; set; }

        public short TF { get; set; }

        public float Vector
        {
            get
            {
                return vector ?? 0;
            }
            set
            {
                if (conStr == null || vector == null)
                {
                    vector = value;
                    return;
                }

                vector = value;

                var cmdStr = $@"UPDATE localterms SET vector = {value}
                                WHERE value = {Value} AND postid = {PostID};";

                using (var con = new NpgsqlConnection(conStr))
                using (var cmd = new NpgsqlCommand(cmdStr, con))
                {
                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }



        public LocalTerm(NpgsqlConnectionStringBuilder connection = null)
        {
            conStr = connection;
        }
    }
}
