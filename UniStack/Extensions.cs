using System;
using System.Collections.Generic;
using System.Linq;

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
        private static readonly Dictionary<string, string> contractions = new Dictionary<string, string>
        {
            // Specific words.
            ["ain't"] = "am not",
            ["aint"] = "am not",
            ["amn't"] = "",
            ["amnt"] = "am not",
            ["can't"] = "cannot",
            ["gonna"] = "going to",
            ["i'm"] = "i am",
            ["im"] = "i am",
            ["it's"] = "it is",
            ["let's"] = "let us",
            ["lets"] = "let us",
            ["ol'"] = "old",
            ["wanna"] = "want to",
            ["y'all"] = "you all",

            // General/misc contractions.
            ["n't"] = " not",
            ["'ve"] = " have",
            ["'ll"] = " will"
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

        public static string ExpandContractions(this string str)
        {
            var expanded = str.ToLowerInvariant();

            foreach (var c in contractions)
            {
                expanded = expanded.Replace(c.Key, c.Value);
            }

            return expanded;
        }



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
