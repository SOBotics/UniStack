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
            ". ", ": ", "; ", "- ", "?", "!", "\\", "/", ",", " "
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
            var lastWordInd = words.Length - 1;
            var lastWord = words[lastWordInd];

            if (strEndPunc.Contains(lastWord[lastWord.Length]))
            {
                words[lastWordInd] = lastWord.Substring(0, lastWord.Length - 1);
            }

            foreach (var wrd in words)
            {
                var w = wrd;

                if (w.All(c => !char.IsLetterOrDigit(c))) continue;

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
