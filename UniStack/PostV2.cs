using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    public class PostV2
    {
        public uint ID { get; set; }
        public float Length { get; set; }
        public Dictionary<string, TermV2> Terms { get; set; }
    }
}
