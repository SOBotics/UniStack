using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    public class TermV2
    {
        public string Term { get; set; }
        public ushort TF { get; set; }
        public float TfIdf { get; set; }
        public float Idf { get; set; }
    }
}
