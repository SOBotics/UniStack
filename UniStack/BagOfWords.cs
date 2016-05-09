using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    /// <summary>
    /// Term weighting: TF-IDF.
    /// Similarity function: cosine.
    /// 
    /// This object is NOT thread-safe.
    /// </summary>
    public class BagOfWords
    {
        private readonly Dictionary<uint, Post> vectorMatrix = new Dictionary<uint, Post>();
        private bool minipulatedSinceLastRecalc = true;



        public bool ContainsPost(uint postID)
        {
            return vectorMatrix.ContainsKey(postID);
        }

        public void AddPost(uint postID, IDictionary<string, ushort> postTFs)
        {
            if (postTFs == null) throw new ArgumentNullException(nameof(postTFs));
            if (ContainsPost(postID))
            {
                throw new ArgumentException("A post with this ID already exists.", nameof(postID));
            }

            var terms = new Dictionary<string, Term>();

            foreach (var t in postTFs)
            {
                terms[t.Key] = new Term
                {
                    TF = t.Value
                };
            }

            vectorMatrix.Add(postID, new Post
            {
                ID = postID,
                Terms = terms
            });

            minipulatedSinceLastRecalc = true;
        }

        public void RemovePost(uint postID)
        {
            if (!ContainsPost(postID))
            {
                throw new KeyNotFoundException("Cannot find any posts with the specified ID.");
            }

            minipulatedSinceLastRecalc = true;

            vectorMatrix.Remove(postID);
        }

        public Dictionary<uint, double> GetSimilarity(IDictionary<string, ushort> queryTerms, uint maxPostsToReturn, double minSimilarity)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateData();
                minipulatedSinceLastRecalc = false;
            }

            // Get the count of the most common term in the query. (Slightly faster than using Linq.)
            var maxQueryTermCount = 0F;
            foreach (var qt in queryTerms)
            {
                if (qt.Value > maxQueryTermCount)
                {
                    maxQueryTermCount = qt.Value;
                }
            }

            // Generate the query's TF-IDF values (vectors).
            var queryTfIdfs = new Dictionary<string, float>();
            foreach (var qt in queryTerms)
            foreach (var p in vectorMatrix.Values)
            {
                if (p.Terms.ContainsKey(qt.Key))
                {
                    queryTfIdfs[qt.Key] = p.Terms[qt.Key].Idf * (qt.Value / maxQueryTermCount);
                    break;
                }
            }

            // Calculate the query's Euclidean length.
            var queryLen = 0F;
            foreach (var tfIdf in queryTfIdfs.Values)
            {
                queryLen += tfIdf * tfIdf;
            }
            queryLen = (float)Math.Sqrt(queryLen);

            // Get the similarities.
            var simResults = new Dictionary<uint, double>();
            foreach (var p in vectorMatrix)
            {
                var sim = 0D;

                foreach (var qt in queryTfIdfs)
                {
                    if (p.Value.Terms.ContainsKey(qt.Key))
                    {
                        //         post TF-IDF vector      * query post TF-IDF vector
                        sim += p.Value.Terms[qt.Key].Vector * qt.Value;
                    }
                }

                sim /= queryLen * p.Value.Length;

                if (sim < minSimilarity) continue;

                simResults[p.Key] = sim;
            }

            // Return only the top x posts.
            var topPosts = new Dictionary<uint, double>();
            var temp = simResults.OrderByDescending(x => x.Value);
            var safeMax = Math.Min(simResults.Count, maxPostsToReturn);
            foreach (var doc in temp)
            {
                if (topPosts.Count == safeMax) break;

                topPosts[doc.Key] = doc.Value;
            }

            return topPosts;
        }



        private void RecalculateData()
        {
            var postCount = (float)vectorMatrix.Count;
            var processedTerms = new HashSet<string>();

            foreach (var id in vectorMatrix.Keys)
            {
                foreach (var termStr in vectorMatrix[id].Terms.Keys)
                {
                    if (processedTerms.Contains(termStr)) continue;

                    // Count how many posts contain the term.
                    var postsContaingTerm = 0;
                    foreach (var p in vectorMatrix)
                    {
                        if (p.Value.Terms.ContainsKey(termStr))
                        {
                            postsContaingTerm++;
                        }
                    }

                    // Calculate the term's IDF.
                    var termIdf = (float)Math.Log(postCount / postsContaingTerm, 2);

                    foreach (var pID in vectorMatrix.Keys)
                    {
                        if (vectorMatrix[pID].Terms.ContainsKey(termStr))
                        {
                            vectorMatrix[pID].Terms[termStr].Idf = termIdf;

                            // And calc the term's TF-IDF vector.
                            vectorMatrix[pID].Terms[termStr].Vector = termIdf * vectorMatrix[pID].Terms[termStr].TF;
                        }
                    }

                    processedTerms.Add(termStr);
                }

                // Now that we have all the terms' TF-IDF vectors,
                // we can calc the post's length.
                var len = 0F;
                foreach (var t in vectorMatrix[id].Terms)
                {
                    len += t.Value.Vector * t.Value.Vector;
                }

                vectorMatrix[id].Length = (float)Math.Sqrt(len);
            }
        }
    }
}
