using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace UniStack.NLP
{
	public static class PostTokeniser
	{
		public class PostToken
		{
			public string Word { get; set; }
			public WordType Type { get; set; }
		}

		public enum WordType
		{
			Unknown,
			None,
			Text,
			Link,
			Header,
			ListItem,
			Blockquote,
			Pre,
			Code,
			InlineCode,
			CodeBlock
		}

		private static string[] wordDelimiters = new[]
		{
			" ",
			"\n"
		};



		public static List<PostToken> Tokenise(string body)
		{
			body = body?.Replace("&nbsp;", " ", StringComparison.Ordinal);
			body = body?.Trim();

			if (string.IsNullOrEmpty(body))
			{
				return null;
			}

			var tokens = new List<PostToken>();
			var tagSplit = SplitTags(body);
			var layers = new List<WordType> { WordType.Text };

			for (var i = 0; i < tagSplit.Count; i++)
			{
				if (tagSplit[i][0] == '<')
				{
					if (tagSplit[i].Length > 1 && tagSplit[i][1] == '/')
					{
						if (layers.Count > 1)
						{
							layers.RemoveAt(layers.Count - 1);
						}

						continue;
					}

					var type = GetTagType(tagSplit[i]);

					if (type != WordType.None)
					{
						layers.Add(type);
					}

					continue;
				}

				var wordSplit = tagSplit[i]
					.Split(wordDelimiters, StringSplitOptions.RemoveEmptyEntries);
				var currentLayer = layers[layers.Count - 1];

				if (currentLayer == WordType.Code)
				{
					if (layers.Count > 1 && layers[layers.Count - 2] == WordType.Pre)
					{
						currentLayer = WordType.CodeBlock;
					}
					else
					{
						currentLayer = WordType.InlineCode;
					}
				}
				else if (layers.Count > 1 &&
					currentLayer == WordType.Text &&
					layers[layers.Count - 2] == WordType.Blockquote)
				{
					currentLayer = WordType.Blockquote;
				}
				else if (currentLayer == WordType.Unknown)
				{
					currentLayer = WordType.Text;
				}

				foreach (var word in wordSplit)
				{
					tokens.Add(new PostToken
					{
						Word = HttpUtility.HtmlDecode(word),
						Type = currentLayer
					});
				}
			}

			return tokens;
		}



		private static WordType GetTagType(string tag)
		{
			var t = tag.Substring(0, 3);

			switch (t)
			{
				case "<p>":
				{
					return WordType.Text;
				}
				case "<em":
				{
					return WordType.Text;
				}
				case "<st":
				{
					return WordType.Text;
				}
				case "<pr":
				{
					return WordType.Pre;
				}
				case "<co":
				{
					return WordType.Code;
				}
				case "<bl":
				{
					return WordType.Blockquote;
				}
				case "<a ":
				{
					return WordType.Link;
				}
				case "<li":
				{
					return WordType.ListItem;
				}
				case "<h1":
				{
					return WordType.Header;
				}
				case "<h2":
				{
					return WordType.Header;
				}
				case "<h3":
				{
					return WordType.Header;
				}
				case "<im":
				{
					return WordType.None;
				}
				case "<br":
				{
					return WordType.None;
				}
				case "<hr":
				{
					return WordType.None;
				}
				default:
				{
					return WordType.Unknown;
				}
			}
		}

		private static List<string> SplitTags(string html)
		{
			var builders = new StringBuilder[html.Length / 2];
			var currentBuilderIndex = -1;

			for (var i = 0; i < html.Length; i++)
			{
				if (html[i] == '<')
				{
					currentBuilderIndex++;
					builders[currentBuilderIndex] = new StringBuilder();
				}

				builders[currentBuilderIndex].Append(html[i]);

				if (html[i] == '>')
				{
					currentBuilderIndex++;
					builders[currentBuilderIndex] = new StringBuilder();
				}
			}

			var splitStrings = new List<string>();

			for (var i = 0; i < currentBuilderIndex + 1; i++)
			{
				var str = builders[i].ToString().Trim();

				if (str.Length == 0) continue;

				splitStrings.Add(str);
			}

			return splitStrings;
		}
	}
}
