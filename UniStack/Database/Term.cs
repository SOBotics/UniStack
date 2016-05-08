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
        public int ID { get; set; }
        public string Value { get; set; }
        public int PostID { get; set; }
        public int TF { get; set; }
        public float Idf { get; set; }
        public float TfIdf { get; set; }
    }
}
