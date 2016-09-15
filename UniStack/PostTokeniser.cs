using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace UniStack
{
    public static class PostTokeniser
    {
        private const string tknDelimiter = "•";
        private const RegexOptions regOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Regex codeBlock = new Regex("(?is)<pre.*?><code>.*?</code></pre>", regOpts);
        private static readonly Regex inlineCode = new Regex("(?is)<code>(.*?)</code>", regOpts);
        private static readonly Regex blockquote = new Regex("(?is)<blockquote>.*?</blockquote>", regOpts);
        private static readonly Regex pic = new Regex("(?is)(<a href=\"\\S+\">)?<img.*?>(</a>)?", regOpts);
        private static readonly Regex link = new Regex("(?is)<a.*?>.*?</a>", regOpts);
        private static readonly Regex htmlTags = new Regex("(?is)<.*?>", regOpts);
        private static readonly Regex nonEng = new Regex(@"[^\x00-\x7F]+", regOpts);
        private static readonly Regex multiWhiteSpace = new Regex(@"\s+", regOpts);
        private static readonly Dictionary<string, string> tkns = new Dictionary<string, string>
        {
            ["code block small"] = AddTknDelimiters("CBS"),
            ["code block medium"] = AddTknDelimiters("CBM"),
            ["code block large"] = AddTknDelimiters("CBL"),
            ["in-line code small"] = AddTknDelimiters("ICS"),
            ["in-line code medium"] = AddTknDelimiters("ICM"),
            ["in-line code large"] = AddTknDelimiters("ICL"),
            ["blockquote small"] = AddTknDelimiters("BQS"),
            ["blockquote medium"] = AddTknDelimiters("BQM"),
            ["blockquote large"] = AddTknDelimiters("BQL"),
            ["link"] = AddTknDelimiters("LLL"),
            ["picture"] = AddTknDelimiters("PPP")
        };



        public static string TokenisePost(string body)
        {
            // Remove any non-English chars + normalise case.
            var tokenised = nonEng.Replace(body, "");
            tokenised = body.ToLowerInvariant();

            // Process the various post segments.
            //tkn = TagBlockQuotes(tkn); // It seems that tokenising these blocks removes a fair amount of entropy.
            tokenised = TokeniseCodeBlocks(tokenised);
            tokenised = TokeniseInlineCode(tokenised);
            tokenised = TokenisePictures(tokenised);
            tokenised = TokeniseLinks(tokenised);

            // Remove any remaining HTML tags.
            tokenised = htmlTags.Replace(tokenised, " ");

            // Now let's try to remove any potentially unformatted code.
            // (Not doing this drastically reduces search accuracy.)
            var lines = tokenised.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var tknCln = "";
            foreach (var l in lines)
            {
                var lCpy = multiWhiteSpace.Replace(l, " ").Trim();
                var p = lCpy.GetPunctuationRatio();
                var w = lCpy.GetWhiteSpaceRatio();

                if ((p >= 0.175 || w <= 0.08) && lCpy.Length > 10)
                {
                    var tokens = "";
                    var tagIndex = lCpy.IndexOf(tknDelimiter);

                    while (tagIndex > 0 && tagIndex + 4 <= lCpy.Length - 1 && lCpy[tagIndex + 4] == tknDelimiter[0])
                    {
                        tokens += lCpy.Substring(tagIndex, 5) + " ";
                        lCpy = lCpy.Remove(tagIndex, 5);

                        tagIndex = lCpy.IndexOf(tknDelimiter);
                    }

                    tknCln += tokens + "\n";
                }
                else
                {
                    tknCln += l + "\n";
                }
            }

            return tknCln;
        }



        private static string AddTknDelimiters(string tagRoot) => tknDelimiter + tagRoot + tknDelimiter;

        private static string TokeniseCodeBlocks(string body)
        {
            var output = body;
            var m = codeBlock.Match(output);

            while (m.Success)
            {
                var code = output.Substring(m.Index, m.Length);
                var lines = code.Split('\n');

                output = output.Remove(m.Index, m.Length);

                if (lines.Length < 6)
                {
                    output = output.Insert(m.Index, tkns["code block small"]);
                }
                else if (lines.Length < 21)
                {
                    output = output.Insert(m.Index, tkns["code block medium"]);
                }
                else
                {
                    output = output.Insert(m.Index, tkns["code block large"]);
                }

                m = codeBlock.Match(output);
            }

            return output;
        }

        private static string TokeniseInlineCode(string body)
        {
            var output = body;
            var m = inlineCode.Match(output);

            while (m.Success)
            {
                var code = output.Substring(m.Index, m.Length);

                output = output.Remove(m.Index, m.Length);

                if (m.Groups[1].Length < 11)
                {
                    output = output.Insert(m.Index, tkns["in-line code small"]);
                }
                else if (m.Groups[1].Length < 36)
                {
                    output = output.Insert(m.Index, tkns["in-line code medium"]);
                }
                else
                {
                    output = output.Insert(m.Index, tkns["in-line code large"]);
                }

                m = inlineCode.Match(output);
            }

            return output;
        }

        private static string TokeniseBlockquotes(string body)
        {
            var output = body;
            var m = blockquote.Match(output);

            while (m.Success)
            {
                var quote = output.Substring(m.Index, m.Length);
                var lines = quote.Split('\n');

                output = output.Remove(m.Index, m.Length);

                if (lines.Length < 4)
                {
                    output = output.Insert(m.Index, tkns["blockquote small"]);
                }
                else if (lines.Length < 11)
                {
                    output = output.Insert(m.Index, tkns["blockquote medium"]);
                }
                else
                {
                    output = output.Insert(m.Index, tkns["blockquote large"]);
                }

                m = blockquote.Match(output);
            }

            return output;
        }

        private static string TokeniseLinks(string body)
        {
            var output = body;
            var m = link.Match(output);

            while (m.Success)
            {
                output = output.Remove(m.Index, m.Length);
                output = output.Insert(m.Index, tkns["link"]);

                m = link.Match(output);
            }

            return output;
        }

        private static string TokenisePictures(string body)
        {
            var output = body;
            var m = pic.Match(output);

            while (m.Success)
            {
                output = output.Remove(m.Index, m.Length);
                output = output.Insert(m.Index, tkns["picture"]);

                m = pic.Match(output);
            }

            return output;
        }
    }
}
