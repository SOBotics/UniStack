using System.Collections.Generic;

namespace UniStack
{
    public class Post
    {
        public float Length { get; set; }
        public Dictionary<string, Term> Terms { get; set; }
    }
}
