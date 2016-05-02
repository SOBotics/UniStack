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
            " (", "(\"", "('", ") ", ").", "),", ")?", ")!", "\")", "')", "):", ");",

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



        // ~30% faster than using a compiled regex.
        public static Dictionary<string, uint> GetTFs(this string str)
        {
            var tfs = new Dictionary<string, uint>();
            var words = str.ToLowerInvariant().Split(wordDelimeters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var wrd in words)
            {
                var w = wrd;

                if (w.All(c => !char.IsLetterOrDigit(c))) continue;

                if (w.EndsWith(".")) w = w.Substring(0, w.Length - 1);

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
