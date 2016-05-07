using System;
using System.Collections.Generic;
using System.Linq;

namespace UniStack
{
    /// <summary>
    /// This class exposes methods for calculating
    /// the cosine similarity of posts. (This implementation
    /// is less RAM intensive [compared to V2], but suffers
    /// from slower processing speeds.)
    /// </summary>
    public class BagOfWordsV1 : IBagOfWords
    {
        private bool minipulatedSinceLastRecalc = true;

        public Dictionary<string, Term> Terms { get; } = new Dictionary<string, Term>();



        public BagOfWordsV1(IDictionary<string, Term> terms, bool idfsCalculated = false)
        {
            Terms = (Dictionary<string, Term>)terms;

            minipulatedSinceLastRecalc = !idfsCalculated;
        }

        public BagOfWordsV1(IEnumerable<Term> terms, bool idfsCalculated = false)
        {
            foreach (var term in terms)
            {
                Terms[term.Value] = term;
            }

            minipulatedSinceLastRecalc = !idfsCalculated;
        }

        public BagOfWordsV1() { }



        public bool ContainsPost(uint postID)
        {
            return Terms.Values.Any(x => x.PostIDsByTFs.ContainsKey(postID));
        }

        public void AddPost(uint postID, IDictionary<string, ushort> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException(nameof(termTFs));
            if (ContainsPost(postID))
            {
                throw new ArgumentException("A post with this ID already exists.", nameof(postID));
            }

            minipulatedSinceLastRecalc = true;

            foreach (var term in termTFs.Keys)
            {
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

        public void RemovePost(uint postID, IDictionary<string, ushort> termTFs)
        {
            if (termTFs == null) throw new ArgumentNullException(nameof(termTFs));
            if (!ContainsPost(postID))
            {
                throw new KeyNotFoundException("Cannot find any posts with the specified ID.");
            }
            if (!termTFs.Keys.All(Terms.ContainsKey))
            {
                throw new KeyNotFoundException("Not all of the specified terms could be found in the current collection.");
            }

            minipulatedSinceLastRecalc = true;

            foreach (var term in termTFs.Keys)
            {
                if (Terms[term].PostIDsByTFs.ContainsKey(postID))
                {
                    if (Terms[term].PostIDsByTFs.Count == 1)
                    {
                        Terms.Remove(term);
                    }
                    else
                    {
                        Terms[term].PostIDsByTFs.Remove(postID);
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
            var totalPosts = new HashSet<uint>();
            foreach (var term in Terms.Values)
            foreach (var postID in term.PostIDsByTFs.Keys)
            {
                if (!totalPosts.Contains(postID))
                {
                    totalPosts.Add(postID);
                }
            }

            var totalPostCount = (double)totalPosts.Count;

            foreach (var term in Terms.Keys)
            {
                // How many posts contain the term?
                var postsFound = Terms[term].PostIDsByTFs.Count;

                Terms[term].IDF = Math.Log(totalPostCount / postsFound, 2);
            }

            minipulatedSinceLastRecalc = false;
        }

        /// <summary>
        /// Calculates the cosine similarity of the given strings (normally words)
        /// compared to the current collection of Terms.
        /// </summary>
        /// <param name="terms">A collection of tokens (i.e., words) for a given string.</param>
        /// <param name="maxDocsToReturn"></param>
        /// <returns>
        /// A dictionary containing a collection of highest matching post
        /// IDs (the key) with their given similarity (the value).
        /// </returns>
        public Dictionary<uint, double> GetSimilarity(IDictionary<string, ushort> terms, uint maxPostsToReturn, double minSimilairty)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateIDFs();
            }

            var queryVector = CalculateQueryTfIdfVector(terms);
            var queryLength = CalculateQueryLength(queryVector);

            // To prevent calculating the similarity of EVERY document,
            // we'll take all the documents which actually contain at least 
            // one of the query's terms.
            var matchingTerms = Terms.Values.Where(x => terms.ContainsKey(x.Value));
            var matchingDocIDs = new HashSet<uint>();
            foreach (var term in matchingTerms)
            foreach (var docID in term.PostIDsByTFs.Keys)
            {
                if (!matchingDocIDs.Contains(docID))
                {
                    matchingDocIDs.Add(docID);
                }
            }

            // Reconstruct the posts from our term collection.
            var docs = new Dictionary<uint, List<string>>();
            foreach (var docID in matchingDocIDs)
            {
                docs[docID] = GetDocument(docID);
            }

            // Calculate the Euclidean lengths of the posts.
            var docLengths = new Dictionary<uint, double>();
            foreach (var docID in docs.Keys)
            {
                docLengths[docID] = CalculatePostLength(docID, docs[docID]);
            }

            // FINALLY, phew! We made it this far. So, now we can
            // actually calculate the cosine similarity of the posts.
            var docSimilarities = new Dictionary<uint, double>();
            foreach (var docID in docs.Keys)
            {
                var sim = 0D;

                foreach (var term in queryVector.Keys)
                {
                    if (docs[docID].Contains(term))
                    {
                        //        query tf-idf   x  the term's idf  x    the term's tf
                        sim += queryVector[term] * (Terms[term].IDF * Terms[term].PostIDsByTFs[docID]);
                    }
                }

                docSimilarities[docID] = sim / Math.Max((queryLength * docLengths[docID]), 1);
            }

            // Now get the top x posts.
            var topDocs = new Dictionary<uint, double>();
            var temp = docSimilarities.OrderByDescending(x => x.Value);
            var safeMax = Math.Min(docSimilarities.Count, maxPostsToReturn);
            foreach (var doc in temp)
            {
                if (topDocs.Count == safeMax) break;

                topDocs[doc.Key] = doc.Value;
            }

            return topDocs;
        }

        private double CalculatePostLength(uint postID, List<string> terms)
        {
            var len = 0D;

            foreach (var term in terms)
            {
                if (Terms.ContainsKey(term))
                {
                    //      the term's IDF * the term's frequency (for this post)
                    len += Terms[term].IDF * Terms[term].PostIDsByTFs[postID] *
                           Terms[term].IDF * Terms[term].PostIDsByTFs[postID];
                }
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

        private Dictionary<string, double> CalculateQueryTfIdfVector(IDictionary<string, ushort> tf)
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

        private List<string> GetDocument(uint docID)
        {
            var terms = new List<string>();

            foreach (var term in Terms.Values)
            {
                if (term.PostIDsByTFs.Keys.Contains(docID))
                {
                    for (var i = 0; i < term.PostIDsByTFs[docID]; i++)
                    {
                        terms.Add(term.Value);
                    }
                }
            }

            return terms;
        }
    }
}
