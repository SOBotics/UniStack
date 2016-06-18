using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    public class PostDB
    {
        public int ID { get; set; }
        public string Tags { get; set; }
        public Dictionary<int, short> Terms { get; set; }
    }
}
