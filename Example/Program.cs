using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Npgsql;
using UniStack;

// Latest Posts.xml dump (as of Sep, 2016) should have 12,350,819 posts.

namespace Example
{
    public class Program
    {
        static int errors = 0;
        static int pProcessed = 0;
        static int tProcessed = 0;
        static BagOfWordsDB bow;
        static HashSet<PostDB> postsToAdd = new HashSet<PostDB>();

        public static void Main(string[] args)
        {
            var con = new NpgsqlConnectionStringBuilder();
            con.Host = "";
            con.Port = 5432;
            con.Username = "";
            con.Password = "";
            con.Database = "";
            bow = new BagOfWordsDB(con);

            InitDB("");

            Console.Read();
        }

        static void InitDB(string xmlPostDumpFile)
        {
            var s = Stopwatch.StartNew();
            var lines = File.ReadLines(xmlPostDumpFile); 

            Task.Run(() =>
            {
                var ppSec = 0;
                var tpSec = 0;
                Thread.Sleep(1000);
                while (true)
                {
                    Console.Clear();
                    Console.Write(
                        "Posts per second: " + (pProcessed - ppSec) +
                        "\nTerms per second: " + (tProcessed - tpSec) +
                        "\nPosts processed: " + pProcessed +
                        "\nTerms processed: " + tProcessed +
                        "\nErrors: " + errors);
                    ppSec = pProcessed;
                    tpSec = tProcessed;
                    Thread.Sleep(1000);
                }
            });

            foreach (var line in lines)
            {
                try
                {
                    AddPost(line);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }

        static void AddPost(string line)
        {
            var postTypeInd = line.IndexOf("PostTypeId=\"1\"");

            if (postTypeInd == -1) return;

            var id = int.Parse(line.Substring(11, postTypeInd - 13));

            var tagsIndex = line.IndexOf("Tags=\"") + 6;
            var tags = WebUtility.HtmlDecode(line.Substring(tagsIndex, line.IndexOf("\"", tagsIndex) - tagsIndex)).Replace("<", "").Replace(">", ",");

            var bodyIndex = line.IndexOf("Body=\"") + 6;
            var body = WebUtility.HtmlDecode(line.Substring(bodyIndex, line.IndexOf("\"", bodyIndex) - bodyIndex));

            var ts = PostTokeniser.TokenisePost(body).ExpandContractions().ToTermFrequencyDictionary();

            if (ts == null || ts.Count == 0) return;

            pProcessed++;
            tProcessed += ts.Count;

            postsToAdd.Add(new PostDB { ID = id, Tags = tags, Terms = ts });

            if (postsToAdd.Count > 999)
            {
                bow.AddPosts(postsToAdd);
                postsToAdd.Clear();
            }
        }
    }
}
