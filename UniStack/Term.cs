using System.Collections.Generic;

namespace UniStack
{
    public class Term
    {
        public Dictionary<uint, uint> PostIDsByTFs { get; set; }

        public string Value { get; set; }

        public double IDF { get; set; }
    }
}
