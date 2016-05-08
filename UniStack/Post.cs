using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    public class Post
    {
        public uint ID { get; set; }
        public float Length { get; set; }
        public Dictionary<string, Term> Terms { get; set; }
    }
}
