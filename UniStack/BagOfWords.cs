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
    /// This was my first attempt at creating
    /// a cosine sim algo for a bag of words
    /// model, this class solely exists for
    /// as a reference. Use the V2 or DB
    /// implementation for better performance.
    /// </summary>
    public class BagOfWords
    {
        public readonly Dictionary<uint, Post> posts = new Dictionary<uint, Post>();
        private bool minipulatedSinceLastRecalc = true;



        public bool ContainsPost(uint postID)
        {
            return posts.ContainsKey(postID);
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

            posts.Add(postID, new Post
            {
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

            posts.Remove(postID);
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
            foreach (var p in posts.Values)
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
            foreach (var p in posts)
            {
                var sim = 0D;

                foreach (var qt in queryTfIdfs)
                {
                    if (p.Value.Terms.ContainsKey(qt.Key))
                    {
                        //          post TF-IDF vector      * query post TF-IDF vector
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
            var postCount = (float)posts.Count;
            var processedTerms = new HashSet<string>();

            foreach (var id in posts.Keys)
            {
                foreach (var termStr in posts[id].Terms.Keys)
                {
                    if (processedTerms.Contains(termStr)) continue;

                    // Count how many posts contain the term.
                    var postsContaingTerm = 0;
                    foreach (var p in posts)
                    {
                        if (p.Value.Terms.ContainsKey(termStr))
                        {
                            postsContaingTerm++;
                        }
                    }

                    // Calculate the term's IDF.
                    var termIdf = (float)Math.Log(postCount / postsContaingTerm, 2);

                    foreach (var pID in posts.Keys)
                    {
                        if (posts[pID].Terms.ContainsKey(termStr))
                        {
                            posts[pID].Terms[termStr].Idf = termIdf;

                            // And calc the term's TF-IDF vector.
                            posts[pID].Terms[termStr].Vector = termIdf * posts[pID].Terms[termStr].TF;
                        }
                    }

                    processedTerms.Add(termStr);
                }

                // Now that we have all the terms' TF-IDF vectors,
                // we can calc the post's length.
                var len = 0F;
                foreach (var t in posts[id].Terms)
                {
                    len += t.Value.Vector * t.Value.Vector;
                }

                posts[id].Length = (float)Math.Sqrt(len);
            }
        }
    }
}
