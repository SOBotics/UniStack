using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    public class DupeClassifier
    {
        private readonly IBagOfWords bow;



        public DupeClassifier(IBagOfWords bagOfWords)
        {
            if (bagOfWords == null) throw new ArgumentNullException(nameof(bagOfWords));

            bow = bagOfWords;
        }



        public Dictionary<uint, double> ClassifyPost(string body)
        {
            throw new NotImplementedException();
        }
    }
}
