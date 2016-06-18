using System;
using System.Text.RegularExpressions;

namespace UniStack
{
    public static class PostTokeniser
    {
        private const RegexOptions regOpts = RegexOptions.Compiled | RegexOptions.CultureInvariant;
        private static readonly Regex codeBlock = new Regex("(?is)<pre.*?><code>.*?</code></pre>", regOpts);
        private static readonly Regex inlineCode = new Regex("(?is)<code>(.*?)</code>", regOpts);
        private static readonly Regex blockQuote = new Regex("(?is)<blockquote>.*?</blockquote>", regOpts);
        private static readonly Regex pic = new Regex("(?is)(<a href=\"\\S+\">)?<img.*?>(</a>)?", regOpts);
        private static readonly Regex link = new Regex("(?is)<a.*?>.*?</a>", regOpts);
        private static readonly Regex htmlTags = new Regex("(?is)<.*?>", regOpts);
        private static readonly Regex nonEng = new Regex(@"[^\x00-\x7F]+", regOpts);
        private static readonly Regex multiWhiteSpace = new Regex(@"\s+", regOpts);



        public static string TokenisePost(string body)
        {
            // Remove any non-English chars + normalise case.
            var tkn = nonEng.Replace(body, "");
            tkn = body.ToLowerInvariant();

            // Process the various post segments.
            //tkn = TagBlockQuotes(tkn); // It seems that tagging these blocks removes quite valuable entropy.
            tkn = TagCodeBlocks(tkn);
            tkn = TagInlineCode(tkn);
            tkn = TagPictures(tkn);
            tkn = TagLinks(tkn);

            // Remove any remaining HTML tags.
            tkn = htmlTags.Replace(tkn, " ");

            // Now let's try to remove any potentially unformatted code.
            // (Not doing this drastically reduces search accuracy.)
            var lines = tkn.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var tknCln = "";
            foreach (var l in lines)
            {
                var lCpy = multiWhiteSpace.Replace(l, " ").Trim();
                var p = lCpy.GetPunctuationRatio();
                var w = lCpy.GetWhiteSpaceRatio();

                if ((p >= 0.175 || w <= 0.08) && lCpy.Length > 10)
                {
                    var tags = "";
                    var tagIndex = lCpy.IndexOf("•");

                    while (tagIndex > 0)
                    {
                        tags += lCpy.Substring(tagIndex, 5) + " ";
                        lCpy = lCpy.Remove(tagIndex, 5);

                        tagIndex = lCpy.IndexOf("•");
                    }

                    tknCln += tags + "\n";
                }
                else
                {
                    tknCln += l + "\n";
                }
            }

            return tknCln;
        }



        private static string TagCodeBlocks(string body)
        {
            var tagged = body;
            var m = codeBlock.Match(tagged);

            while (m.Success)
            {
                var code = tagged.Substring(m.Index, m.Length);
                var lines = code.Split('\n');

                tagged = tagged.Remove(m.Index, m.Length);

                if (lines.Length < 6)
                {
                    tagged = tagged.Insert(m.Index, " •CBS• ");
                }
                else if (lines.Length < 21)
                {
                    tagged = tagged.Insert(m.Index, " •CBM• ");
                }
                else
                {
                    tagged = tagged.Insert(m.Index, " •CBL• ");
                }

                m = codeBlock.Match(tagged);
            }

            return tagged;
        }

        private static string TagInlineCode(string body)
        {
            var tagged = body;
            var m = inlineCode.Match(tagged);

            while (m.Success)
            {
                var code = tagged.Substring(m.Index, m.Length);

                tagged = tagged.Remove(m.Index, m.Length);

                if (m.Groups[1].Length < 11)
                {
                    tagged = tagged.Insert(m.Index, " •ICS• ");
                }
                else if (m.Groups[1].Length < 36)
                {
                    tagged = tagged.Insert(m.Index, " •ICM• ");
                }
                else
                {
                    tagged = tagged.Insert(m.Index, " •ICL• ");
                }

                m = inlineCode.Match(tagged);
            }

            return tagged;
        }

        private static string TagBlockQuotes(string body)
        {
            var tagged = body;
            var m = blockQuote.Match(tagged);

            while (m.Success)
            {
                var quote = tagged.Substring(m.Index, m.Length);
                var lines = quote.Split('\n');

                tagged = tagged.Remove(m.Index, m.Length);

                if (lines.Length < 4)
                {
                    tagged = tagged.Insert(m.Index, " •BQS• ");
                }
                else if (lines.Length < 11)
                {
                    tagged = tagged.Insert(m.Index, " •BQM• ");
                }
                else
                {
                    tagged = tagged.Insert(m.Index, " •BQL• ");
                }

                m = blockQuote.Match(tagged);
            }

            return tagged;
        }

        private static string TagLinks(string body)
        {
            var tagged = body;
            var m = link.Match(tagged);

            while (m.Success)
            {
                tagged = tagged.Remove(m.Index, m.Length);
                tagged = tagged.Insert(m.Index, " •LLL• ");

                m = link.Match(tagged);
            }

            return tagged;
        }

        private static string TagPictures(string body)
        {
            var tagged = body;
            var m = pic.Match(tagged);

            while (m.Success)
            {
                tagged = tagged.Remove(m.Index, m.Length);
                tagged = tagged.Insert(m.Index, " •PPP• ");

                m = pic.Match(tagged);
            }

            return tagged;
        }
    }
}
