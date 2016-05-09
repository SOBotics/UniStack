namespace UniStack.Database
{
    public class Term
    {
        public long ID { get; set; }
        public string Value { get; set; }
        public int PostID { get; set; }
        public int TF { get; set; }
        public float Idf { get; set; }
        public float Vector { get; set; }
    }
}
