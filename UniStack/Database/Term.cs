using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack.Database
{
    public class Term
    {
        [Key]
        public string Value { get; set; }
        public Dictionary<uint, uint> PostIDsByTFs { get; set; }
        public double Idf { get; set; }
    }
}
