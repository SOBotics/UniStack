using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace UniStack
{
    public static class Extensions
    {
        private static string[] wordDelimeters = new[]
        {
            // Parenthesis.
            ". (", " (", "(\"", "('", ") ", ").", "),", ")?", ")!", "\")", "')", "):", ");",

            // Square brackets.
            " [", "[\"", "['", "] ", "].", "],", "]?", "]!", "\"]", "']", "]:", "];",

            // Curly braces.
            " {", "{\"", "{'", "} ", "}.", "},", "}?", "}!", "\"}", "'}", "}:", "};",

            // Double quotes.
            "=\"", " \"", "\" ", "\".", "\",", "\"?", "\"!", "\":", "\";",

            // Single quotes.
            " '", "' ", "'.", "',", "'?", "'!", "':", "';",

            // Misc.
            ". ", ": ", "; ", "- ", "=", "?", "!", "\\", "/", ",", " ", "\r", "\n", "\t"
        };
        private static readonly char[] strEndPunc = new[]
        {
            '.', ')', '(', ':', '"'
        };
        private static readonly char[] strStartPunc = new[]
        {
            '*'
        };



        // Faster than using a compiled regex.
        public static Dictionary<string, ushort> ToTermFrequencyDictionary(this string str)
        {
            var tfs = new Dictionary<string, ushort>();
            var words = str.ToLowerInvariant().Split(wordDelimeters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var wrd in words)
            {
                var w = wrd;

                if (w.All(c => !char.IsLetterOrDigit(c))) continue;

                w = TrimStart(w);
                w = TrimEnd(w);

                if (tfs.ContainsKey(w))
                {
                    tfs[w]++;
                }
                else
                {
                    tfs[w] = 1;
                }
            }

            return tfs;
        }

        public static double GetPunctuationRatio(this string str)
        {
            var punctCharCount = 0D;
            var wordCharCount = 0;

            foreach (var c in str)
            {
                if (char.IsPunctuation(c))
                {
                    punctCharCount++;
                }
                else if (char.IsLetterOrDigit(c))
                {
                    wordCharCount++;
                }
            }

            return punctCharCount / wordCharCount;
        }

        //public static byte[] Compress(this string str)
        //{
        //    var strBytes = Encoding.UTF8.GetBytes(str);
        //    byte[] zipped;

        //    using (var compStrm = new MemoryStream())
        //    {
        //        using (var zipper = new GZipStream(compStrm, CompressionMode.Compress))
        //        using (var ms = new MemoryStream(strBytes))
        //        {
        //            ms.CopyTo(zipper);
        //        }

        //        zipped = compStrm.ToArray();
        //    }

        //    return zipped;
        //}

        //public static string Decompress(this byte[] bytes)
        //{
        //    using (var msIn = new MemoryStream(bytes))
        //    using (var unzipper = new GZipStream(msIn, CompressionMode.Decompress))
        //    using (var msOut = new MemoryStream())
        //    {
        //        unzipper.CopyTo(msOut);
        //        var unzipped = msOut.ToArray();
        //        return Encoding.UTF8.GetString(unzipped);
        //    }
        //}



        private static string TrimEnd(string str)
        {
            var charsToRem = 0;
            for (var i = str.Length - 1; i > 0; i--)
            {
                if (strEndPunc.Contains(str[i]))
                {
                    charsToRem++;
                }
                else
                {
                    break;
                }
            }

            return str.Substring(0, str.Length - charsToRem);
        }

        private static string TrimStart(string str)
        {
            var charsToRem = 0;
            for (var i = 0; i < str.Length - 1; i++)
            {
                if (strStartPunc.Contains(str[i]))
                {
                    charsToRem++;
                }
                else
                {
                    break;
                }
            }

            return str.Remove(0, charsToRem);
        }
    }
}
