using System;
using System.Collections.Generic;
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
            " \"", "\" ", "\".", "\",", "\"?", "\"!", "\":", "\";",

            // Single quotes.
            " '", "' ", "'.", "',", "'?", "'!", "':", "';",

            // Misc.
            ". ", ": ", "; ", "- ", "?", "!", "\\", "/", ",", " ", "\r", "\n", "\t"
        };
        private static readonly char[] strEndPunc = new[]
        {
            '.', ')'
        };



        // ~30% faster than using a compiled regex.
        public static Dictionary<string, uint> ToTermFrequencyDictionary(this string str)
        {
            var tfs = new Dictionary<string, uint>();
            var words = str.ToLowerInvariant().Split(wordDelimeters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var wrd in words)
            {
                var w = wrd;

                if (w.All(c => !char.IsLetterOrDigit(c))) continue;

                var charsToRem = 0;
                for (var i = w.Length - 1; i > 0; i--)
                {
                    if (strEndPunc.Contains(w[i]))
                    {
                        charsToRem++;
                    }
                    else
                    {
                        break;
                    }
                }

                w = w.Substring(0, w.Length - charsToRem);

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
    }
}
