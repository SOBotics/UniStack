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
            //". (", " (", "(\"", "('", ") ", ").", "),", ")?", ")!", "\")", "')", "):", ");",

            // Square brackets.
            " [", "[\"", "['", "] ", "].", "],", "]?", "]!", "\"]", "']", "]:", "];",

            // Curly braces.
            " {", "{\"", "{'", "} ", "}.", "},", "}?", "}!", "\"}", "'}", "}:", "};",

            // Double quotes.
            "=\"", " \"", "\" ", "\".", "\",", "\"?", "\"!", "\":", "\";",

            // Single quotes.
            " '", "' ", "'.", "',", "'?", "'!", "':", "';",

            // Misc.
            ". ", ": ", "; ", "- ", "**", "=", "?", "!", "\\", "/", ",", ")", "(", " ", "\r", "\n", "\t"
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
        public static Dictionary<int, short> ToTermFrequencyDictionary(this string str)
        {
            var tfs = new Dictionary<int, short>();
            var words = str.ToLowerInvariant().Split(wordDelimeters, StringSplitOptions.RemoveEmptyEntries);

            foreach (var wrd in words)
            {
                var w = wrd;

                if (w.All(c => !char.IsLetterOrDigit(c))) continue;

                w = TrimStart(w);
                w = TrimEnd(w);
                w = Americanise.Apply(w);

                var hash = w.GetStringHashCode();

                if (tfs.ContainsKey(hash))
                {
                    tfs[hash]++;
                }
                else
                {
                    tfs[hash] = 1;
                }
            }

            return tfs;
        }

        public static double GetPunctuationRatio(this string str)
        {
            var punctCount = 0D;

            foreach (var c in str)
            {
                if (char.IsPunctuation(c))
                {
                    punctCount++;
                }
            }

            return punctCount / str.Length;
        }

        public static double GetWhiteSpaceRatio(this string str)
        {
            var whiteSpaceCount = 0D;

            foreach (var c in str)
            {
                if (char.IsWhiteSpace(c))
                {
                    whiteSpaceCount++;
                }
            }

            return whiteSpaceCount / str.Length;
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

        public static int GetStringHashCode(this string str)
        {
            var hash = 23;

            foreach (var c in str)
            {
                hash = hash * 33 + c;
            }

            return hash;
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
