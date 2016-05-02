using System.Collections.Generic;

namespace UniStack
{
    public class Post
    {
        public uint ID { get; set; }
        public Dictionary<string, uint> TermsByTFs { get; set; }
    }
}
