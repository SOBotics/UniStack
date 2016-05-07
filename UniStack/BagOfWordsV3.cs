using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniStack
{
    public class BagOfWordsV3 : IBagOfWords
    {
        private readonly Dictionary<uint, PostV2> tfIdfMatrix = new Dictionary<uint, PostV2>();
        private bool minipulatedSinceLastRecalc = true;



        public bool ContainsPost(uint postID)
        {
            return tfIdfMatrix.ContainsKey(postID);
        }

        public void AddPost(uint postID, IDictionary<string, ushort> postTFs)
        {
            if (postTFs == null) throw new ArgumentNullException(nameof(postTFs));
            if (ContainsPost(postID))
            {
                throw new ArgumentException("A post with this ID already exists.", nameof(postID));
            }

            var terms = new Dictionary<string, TermV2>();

            foreach (var t in postTFs)
            {
                terms[t.Key] = new TermV2
                {
                    Term = t.Key,
                    TF = t.Value
                };
            }

            tfIdfMatrix.Add(postID, new PostV2
            {
                ID = postID,
                Terms = terms
            });

            minipulatedSinceLastRecalc = true;
        }

        public void RemovePost(uint postID, IDictionary<string, ushort> postTFs = null) //TODO: We should really just remove this param.
        {
            if (!ContainsPost(postID))
            {
                throw new KeyNotFoundException("Cannot find any posts with the specified ID.");
            }

            minipulatedSinceLastRecalc = true;

            tfIdfMatrix.Remove(postID);
        }

        public Dictionary<uint, double> GetSimilarity(IDictionary<string, ushort> queryTerms, uint maxPostsToReturn, double minSimilarity)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateData();
            }

            // Generate the query's TF-IDF values.
            var queryTfIdfs = new Dictionary<string, float>();
            var maxQueryTermCount = (float)queryTerms.Max(x => x.Value);
            foreach (var qt in queryTerms)
            foreach (var p in tfIdfMatrix.Values)
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
            foreach (var p in tfIdfMatrix.Values)
            {
                var sim = 0D;

                foreach (var qt in queryTfIdfs)
                foreach (var t in p.Terms.Values)
                {
                    if (t.Term == qt.Key)
                    {
                        sim += t.TfIdf * qt.Value;
                    }
                }

                sim /= queryLen * p.Length;

                if (sim < minSimilarity) continue;

                simResults[p.ID] = sim;
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
            var postCount = (float)tfIdfMatrix.Count;
            var processedTerms = new HashSet<string>();

            foreach (var id in tfIdfMatrix.Keys)
            {
                foreach (var termStr in tfIdfMatrix[id].Terms.Keys)
                {
                    if (processedTerms.Contains(termStr)) continue;

                    // Count how many posts contain the term.
                    var postsContaingTerm = 0;
                    foreach (var p in tfIdfMatrix)
                    foreach (var t in p.Value.Terms.Keys)
                    {
                        if (t == termStr)
                        {
                            postsContaingTerm++;
                            break;
                        }
                    }

                    // Calculate the term's IDF.
                    var termIdf = (float)Math.Log(postCount / postsContaingTerm, 2);

                    foreach (var pID in tfIdfMatrix.Keys)
                    foreach (var t in tfIdfMatrix[pID].Terms.Keys)
                    {
                        if (t == termStr)
                        {
                            tfIdfMatrix[pID].Terms[t].Idf = termIdf;

                            // And calc the term's TF-IDF vector.
                            tfIdfMatrix[pID].Terms[t].TfIdf = termIdf * tfIdfMatrix[pID].Terms[t].TF;
                            break;
                        }
                    }

                    processedTerms.Add(termStr);
                }

                // Now that we have all the terms' TF-IDF vectors,
                // we can calc the post's length.
                var len = 0F;
                foreach (var t in tfIdfMatrix[id].Terms)
                {
                    len += t.Value.TfIdf * t.Value.TfIdf;
                }

                tfIdfMatrix[id].Length = (float)Math.Sqrt(len);
            }
        }
    }
}
