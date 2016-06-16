using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    public class TermV2
    {
        public float Idf { get; set; }

        public Dictionary<int, byte> PostIDByCount { get; set; }
    }
}
