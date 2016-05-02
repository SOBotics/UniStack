using System;
using System.Collections.Generic;
using System.Linq;

namespace UniStack
{
    /// <summary>
    /// This class exposes methods for calculating
    /// the cosine similarity of posts. (This implementation
    /// is more RAM intensive [compared to V1], but benefits
    /// from greater processing speeds.)
    /// </summary>
    public class BagOfWordsV2 : IBagOfWords
    {
        private bool minipulatedSinceLastRecalc = true;

        public Dictionary<string, Term> Terms { get; } = new Dictionary<string, Term>();

        public Dictionary<uint, Post> Posts { get; } = new Dictionary<uint, Post>();



        public BagOfWordsV2(IEnumerable<Term> terms, bool idfsCalculated = false)
        {
            foreach (var term in terms)
            {
                Terms[term.Value] = term;

                foreach (var postIDByTF in term.PostIDsByTFs)
                {
                    if (Posts.ContainsKey(postIDByTF.Key))
                    {
                        Posts[postIDByTF.Key].TermsByTFs[term.Value] = postIDByTF.Value;
                    }
                    else
                    {
                        Posts[postIDByTF.Key] = new Post
                        {
                            ID = postIDByTF.Key,
                            TermsByTFs = new Dictionary<string, uint>
                            {
                                [term.Value] = postIDByTF.Value
                            }
                        };
                    }
                }
            }

            minipulatedSinceLastRecalc = !idfsCalculated;
        }

        public BagOfWordsV2() { }



        public bool ContainsPost(uint docID)
        {
            return Posts.ContainsKey(docID);
        }

        public void AddPost(uint postID, IDictionary<string, uint> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException(nameof(termTFs));
            if (ContainsPost(postID))
            {
                throw new ArgumentException("A post with this ID already exists.", nameof(postID));
            }

            minipulatedSinceLastRecalc = true;

            Posts[postID] = new Post
            {
                ID = postID,
                TermsByTFs = new Dictionary<string, uint>()
            };

            foreach (var term in termTFs.Keys)
            {
                Posts[postID].TermsByTFs[term] = termTFs[term];

                if (Terms.ContainsKey(term))
                {
                    Terms[term].PostIDsByTFs[postID] = termTFs[term];
                }
                else
                {
                    Terms[term] = new Term
                    {
                        PostIDsByTFs = new Dictionary<uint, uint>
                        {
                            [postID] = termTFs[term]
                        },
                        Value = term
                    };
                }
            }
        }

        public void RemovePost(uint PostID, IDictionary<string, uint> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException(nameof(termTFs));
            if (!ContainsPost(PostID))
            {
                throw new KeyNotFoundException("Cannot find any posts with the specified ID.");
            }
            if (!termTFs.Keys.All(Terms.ContainsKey))
            {
                throw new KeyNotFoundException("Not all the of specified terms were found in the current collection.");
            }

            minipulatedSinceLastRecalc = true;

            Posts.Remove(PostID);

            foreach (var term in termTFs.Keys)
            {
                if (Terms[term].PostIDsByTFs.ContainsKey(PostID))
                {
                    if (Terms[term].PostIDsByTFs.Count == 1)
                    {
                        Terms.Remove(term);
                    }
                    else
                    {
                        Terms[term].PostIDsByTFs.Remove(PostID);
                    }
                }
            }
        }

        public void RecalculateIDFs()
        {
            foreach (var term in Terms.Keys)
            {
                Terms[term].IDF = 0;
            }

            // Get all the post IDs.
            var totalDocs = new HashSet<uint>();
            foreach (var term in Terms.Values)
            foreach (var docID in term.PostIDsByTFs.Keys)
            {
                if (!totalDocs.Contains(docID))
                {
                    totalDocs.Add(docID);
                }
            }

            var totalDocCount = (float)totalDocs.Count;

            foreach (var term in Terms.Keys)
            {
                // How many posts contain the term?
                var docsFound = Terms[term].PostIDsByTFs.Count;

                Terms[term].IDF = (float)Math.Log(totalDocCount / docsFound, 2);
            }

            minipulatedSinceLastRecalc = false;
        }

        /// <summary>
        /// Calculates the cosine similarity of the given strings (normally words)
        /// compared to the current collection of Terms.
        /// </summary>
        /// <param name="terms">A collection of tokens (i.e., words) for a given string.</param>
        /// <param name="maxPostsToReturn"></param>
        /// <returns>
        /// A dictionary containing a collection of highest
        /// matching post IDs (the key) with their given similarity (the value).
        /// </returns>
        public Dictionary<uint, double> GetSimilarity(IDictionary<string, uint> terms, uint maxPostsToReturn)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateIDFs();
            }

            var queryVector = CalculateQueryTfIdfVector(terms);
            var queryLength = CalculateQueryLength(queryVector);

            // To prevent calculating the similarity of EVERY post,
            // we'll take all the posts which actually contain at least 
            // one of the query's terms.
            var matchingTerms = Terms.Values.Where(x => terms.Keys.Any(y => y == x.Value));
            var matchingPostIDs = new HashSet<uint>();
            foreach (var term in matchingTerms)
            foreach (var postID in term.PostIDsByTFs.Keys)
            {
                if (!matchingPostIDs.Contains(postID))
                {
                    matchingPostIDs.Add(postID);
                }
            }

            // Calculate the Euclidean lengths of the posts.
            var docLengths = new Dictionary<uint, double>();
            foreach (var postID in matchingPostIDs)
            {
                docLengths[postID] = CalculatePostLength(Posts[postID].TermsByTFs);
            }

            // FINALLY, phew! We made it this far. So, now we can
            // actually calculate the cosine similarity for the posts.
            var postSimilarities = new Dictionary<uint, double>();
            foreach (var docID in matchingPostIDs)
            {
                var sim = 0D;

                foreach (var term in queryVector.Keys)
                {
                    if (Posts[docID].TermsByTFs.ContainsKey(term))
                    {
                        //        query tf-idf   x  the term's idf  x    the term's tf
                        sim += queryVector[term] * (Terms[term].IDF * Terms[term].PostIDsByTFs[docID]);
                    }
                }

                postSimilarities[docID] = sim / Math.Max((queryLength * docLengths[docID]), 1);
            }

            // Now get the top x docs.
            var topPosts = new Dictionary<uint, double>();
            var temp = postSimilarities.OrderByDescending(x => x.Value);
            var safeMax = Math.Min(postSimilarities.Count, maxPostsToReturn);
            foreach (var doc in temp)
            {
                if (topPosts.Count == safeMax) break;

                topPosts[doc.Key] = doc.Value;
            }

            return topPosts;
        }

        private double CalculatePostLength(Dictionary<string, uint> tfs)
        {
            var len = 0D;

            foreach (var tf in tfs)
            {
                len += (Terms[tf.Key].IDF * tf.Value *
                        Terms[tf.Key].IDF * tf.Value) *
                        tf.Value;
            }

            return Math.Sqrt(len);
        }

        private double CalculateQueryLength(Dictionary<string, double> queryVector)
        {
            var len = 0D;

            foreach (var tfidf in queryVector.Values)
            {
                len += tfidf * tfidf;
            }

            return Math.Sqrt(len);
        }

        private Dictionary<string, double> CalculateQueryTfIdfVector(IDictionary<string, uint> tf)
        {
            var maxFrec = (double)tf.Max(x => x.Value);

            var tfIdf = new Dictionary<string, double>();

            foreach (var term in tf.Keys)
            {
                if (Terms.ContainsKey(term))
                {
                    tfIdf[term] = (maxFrec / tf[term]) * Terms[term].IDF;
                }
            }

            return tfIdf;
        }
    }
}
