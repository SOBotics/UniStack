using System;
using System.Collections.Generic;
using System.Linq;

namespace UniStack
{
    /// <summary>
    /// Term weighting: TF-IDF.
    /// Similarity function: cosine.
    /// 
    /// This object is NOT thread-safe.
    /// 
    /// This implementation is optimised for minimal memory
    /// usage.
    /// </summary>
    public class BagOfWordsV2
    {
        public readonly Dictionary<int, TermV2> termsByTfIdf = new Dictionary<int, TermV2>();
        public readonly Dictionary<int, float> postsByLength = new Dictionary<int, float>();
        private bool minipulatedSinceLastRecalc = true;



        public bool ContainsPost(int postID)
        {
            return postsByLength.ContainsKey(postID);
        }

        public void AddPost(int postID, IDictionary<int, ushort> termHashesByCount)
        {
            if (termHashesByCount == null) throw new ArgumentNullException(nameof(termHashesByCount));
            if (ContainsPost(postID))
            {
                throw new ArgumentException("A post with this ID already exists.", nameof(postID));
            }

            var terms = new Dictionary<string, Term>();

            foreach (var t in termHashesByCount)
            {
                if (t.Value > byte.MaxValue) throw new Exception();

                if (!termsByTfIdf.ContainsKey(t.Key))
                {
                    termsByTfIdf[t.Key] = new TermV2
                    {
                        PostIDByCount = new Dictionary<int, byte>
                        {
                            [postID] = (byte)t.Value
                        }
                    };
                }
                else
                {
                    termsByTfIdf[t.Key].PostIDByCount[postID] = (byte)t.Value;
                }
            }

            postsByLength[postID] = -1;

            minipulatedSinceLastRecalc = true;
        }

        public void RemovePost(int postID)
        {
            if (!ContainsPost(postID))
            {
                throw new KeyNotFoundException("Cannot find any posts with the specified ID.");
            }

            minipulatedSinceLastRecalc = true;

            postsByLength.Remove(postID);

            //TODO: Remove from termsByTfIdf.
        }

        public Dictionary<int, double> GetSimilarity(IDictionary<int, ushort> queryTermHashesByCount, uint maxPostsToReturn, double minSimilarity)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateData();
                minipulatedSinceLastRecalc = false;
            }

            // Get the count of the most common term in the query. (Slightly faster than using Linq.)
            var maxQueryTermCount = 0F;
            foreach (var qt in queryTermHashesByCount)
            {
                if (qt.Value > maxQueryTermCount)
                {
                    maxQueryTermCount = qt.Value;
                }
            }

            // Generate the query's TF-IDF values (vectors).
            var queryTermHashesByTfIdfVector = new Dictionary<int, float>();
            foreach (var qt in queryTermHashesByCount)
            {
                if (!termsByTfIdf.ContainsKey(qt.Key)) continue;

                queryTermHashesByTfIdfVector[qt.Key] = termsByTfIdf[qt.Key].Idf * (qt.Value / maxQueryTermCount);
            }

            // Calculate the query's Euclidean length.
            var queryLen = 0F;
            foreach (var tfIdf in queryTermHashesByTfIdfVector.Values)
            {
                queryLen += tfIdf * tfIdf;
            }
            queryLen = (float)Math.Sqrt(queryLen);

            // Get the similarities.
            var simResults = new Dictionary<int, double>();
            foreach (var queryTerm in queryTermHashesByTfIdfVector)
            {
                if (!termsByTfIdf.ContainsKey(queryTerm.Key)) continue;

                foreach (var postID in postsByLength.Keys)
                {
                    if (simResults.ContainsKey(postID) ||
                        !termsByTfIdf[queryTerm.Key].PostIDByCount.ContainsKey(postID))
                    {
                        continue;
                    }

                    var termVector = termsByTfIdf[queryTerm.Key].Idf * termsByTfIdf[queryTerm.Key].PostIDByCount[postID];

                    var sim = (termVector * queryTerm.Value) / (queryLen * postsByLength[postID]);

                    if (sim < minSimilarity) continue;

                    simResults[postID] = sim;
                }
            }

            // Return only the top x posts.
            var topPosts = new Dictionary<int, double>();
            var temp = simResults.OrderByDescending(x => x.Value);
            var safeMax = Math.Min(simResults.Count, maxPostsToReturn);
            foreach (var post in temp)
            {
                if (topPosts.Count == safeMax) break;

                topPosts[post.Key] = post.Value;
            }

            return topPosts;
        }



        private void RecalculateData()
        {
            var postCount = (float)postsByLength.Count;
            var processedTerms = new HashSet<string>();

            foreach (var termID in termsByTfIdf.Keys)
            {
                var idf = Math.Log(postCount / termsByTfIdf[termID].PostIDByCount.Count, 2);
                termsByTfIdf[termID].Idf = (float)idf;
            }

            var postsProcessed = 0;
            for (var postID = 0; postsProcessed < postsByLength.Count; postID++)
            {
                if (!postsByLength.ContainsKey(postID)) continue;

                var len = 0F;

                foreach (var term in termsByTfIdf.Values)
                {
                    if (!term.PostIDByCount.ContainsKey(postID)) continue;

                    var termVector = term.Idf * term.PostIDByCount[postID];

                    len += termVector * termVector;
                }

                postsByLength[postID] = (float)Math.Sqrt(len);

                postsProcessed++;
            }
        }
    }
}
