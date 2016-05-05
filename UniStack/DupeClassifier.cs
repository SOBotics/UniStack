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
            var tfs = body.ToTermFrequencyDictionary();
            //TODO: Tokenise body html tags before calculating similarity.
            return bow.GetSimilarity(tfs, 1);
        }
    }
}
