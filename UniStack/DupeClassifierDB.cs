using System;
using System.Collections.Generic;

namespace UniStack
{
    public class DupeClassifierDB
    {
        private readonly BagOfWordsDB bow;

        public BagOfWordsDB BagOfWordsDB { get { return bow; } }



        public DupeClassifierDB(BagOfWordsDB bagOfWords)
        {
            if (bagOfWords == null) throw new ArgumentNullException(nameof(bagOfWords));

            bow = bagOfWords;
        }



        public Dictionary<int, double> ClassifyPost(string body, string topTag)
        {
            //var tk = PostTokeniser.TokenisePost(body);
            //var expanded = tk.ExpandContractions();
            //var tfs = expanded.ToTermFrequencyDictionary();
            return null;// bow.GetSimilarity(tfs, topTag);
        }
    }
}
