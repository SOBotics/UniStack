using System.Collections.Generic;

namespace UniStack
{
    public class Post
    {
        public uint ID { get; set; }
        public float Length { get; set; }
        public Dictionary<string, Term> Terms { get; set; }
    }
}
