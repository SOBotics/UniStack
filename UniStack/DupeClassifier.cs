using System;
using System.Collections.Generic;

namespace UniStack
{
    public class DupeClassifier
    {
        private readonly BagOfWords bow;

        public BagOfWords BagOfWords { get { return bow; } }



        public DupeClassifier(BagOfWords bagOfWords)
        {
            if (bagOfWords == null) throw new ArgumentNullException(nameof(bagOfWords));

            bow = bagOfWords;
        }



        public Dictionary<uint, double> ClassifyPost(string body)
        {
            var tk = PostTokeniser.TokenisePost(body);
            var expanded = tk.ExpandContractions();
            var tfs = expanded.ToTermFrequencyDictionary();
            return null;// bow.GetSimilarity(tfs, int.MaxValue, 0);
        }
    }
}
