using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.Entity;
using UniStack.Database;

namespace UniStack
{
    /// <summary>
    /// Term weighting: TF-IDF.
    /// Similarity function: cosine.
    /// 
    /// This object is NOT thread-safe.
    /// </summary>
    public class BagOfWordsDB
    {
        private bool minipulatedSinceLastRecalc = true;



        public BagOfWordsDB()
        {
            var db = new DB();

            db.Database.EnsureCreated();
        }



        public bool ContainsPost(int postID)
        {
            using (var db = new DB())
            {
                return db.Posts.Any(x => x.PostID == postID);
            }
        }

        public void AddPost(int postID, string tags, IDictionary<string, ushort> postTFs)
        {
            if (postTFs == null) throw new ArgumentNullException(nameof(postTFs));
            if (ContainsPost(postID))
            {
                throw new ArgumentException("A post with this ID already exists.", nameof(postID));
            }

            minipulatedSinceLastRecalc = true;

            using (var db = new DB())
            {
                foreach (var t in postTFs)
                {
                    db.Terms.Add(new Database.Term
                    {
                        PostID = postID,
                        Value = t.Key,
                        TF = t.Value
                    });
                }

                db.Posts.Add(new Database.Post
                {
                    PostID = postID,
                    Tags = tags
                });

                db.SaveChanges();
            }
        }

        public void RemovePost(int postID)
        {
            if (!ContainsPost(postID))
            {
                throw new KeyNotFoundException("Cannot find any posts with the specified ID.");
            }

            minipulatedSinceLastRecalc = true;

            using (var db = new DB())
            {
                var p = db.Posts.Single(x => x.PostID == postID);
                db.Posts.Remove(p);
                db.SaveChanges();
            }
        }

        public Dictionary<int, double> GetSimilarity(IDictionary<string, ushort> queryTerms, string postTopTag, uint maxPostsToReturn = uint.MaxValue, double minSimilarity = 0)
        {
            if (minipulatedSinceLastRecalc)
            {
                RecalculateData();
                minipulatedSinceLastRecalc = false;
            }

            using (var db = new DB())
            {
                var postsWithTag = db.Posts.Include(p => p.Terms).Where(x => x.Tags.Contains(postTopTag));

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
                foreach (var p in postsWithTag)
                {
                    var t = p.Terms.SingleOrDefault(x => x.Value == qt.Key);

                    if (t != null)
                    {
                        queryTfIdfs[qt.Key] = t.Idf * (qt.Value / maxQueryTermCount);
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
                var simResults = new Dictionary<int, double>();
                foreach (var p in postsWithTag)
                {
                    var sim = 0D;

                    foreach (var qt in queryTfIdfs)
                    {
                        var t = p.Terms.SingleOrDefault(x => x.Value == qt.Key);

                        if (t != null)
                        {
                            // post TF-IDF vector * query post TF-IDF vector
                            sim += t.Vector * qt.Value;
                        }
                    }

                    sim /= queryLen * p.Length;

                    if (sim < minSimilarity) continue;

                    simResults[p.PostID] = sim;
                }

                // Return only the top x posts.
                var topPosts = new Dictionary<int, double>();
                var temp = simResults.OrderByDescending(x => x.Value);
                var safeMax = Math.Min(simResults.Count, maxPostsToReturn);
                foreach (var doc in temp)
                {
                    if (topPosts.Count == safeMax) break;

                    topPosts[doc.Key] = doc.Value;
                }

                return topPosts;
            }
        }



        private void RecalculateData()
        {
            using (var db = new DB())
            {
                var postCount = (float)db.Posts.Count();

                var termsByPostCount = db.Terms
                    .GroupBy(x => x.Value)
                    .Select(x => x.First())
                    .Select(x => new
                     {
                         Term = x,
                         Count = db.Posts.Count(p => p.Terms.Any(t => t.Value == x.Value))
                     });

                foreach (var term in termsByPostCount)
                {
                    var termIdf = (float)Math.Log(postCount / term.Count, 2);

                    foreach (var p in db.Posts.Include(p => p.Terms))
                    {
                        var t = p.Terms.SingleOrDefault(x => x.Value == term.Term.Value);

                        if (t != null)
                        {
                            t.Idf = termIdf;
                            t.Vector = termIdf * t.TF; 
                        }
                    }
                }

                foreach (var p in db.Posts)
                {
                    var len = 0F;

                    foreach (var t in p.Terms)
                    {
                        len += t.Vector * t.Vector;
                    }

                    p.Length = (float)Math.Sqrt(len);
                }

                db.SaveChanges();
            }
        }
    }
}
