using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Data.HashFunction.xxHash;
using System.Linq;
using System.Text;
using System.Web;

namespace UniStack.Data
{
	public static class ModelBuilder
	{
		public class Model
		{
			public int Id;
			public int[] Tags;
			public Dictionary<int, byte> Terms;
		}

		public class PostToken
		{
			public string Word;
			public WordType Type;
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

		private static IxxHash hash = xxHashFactory.Instance.Create(new xxHashConfig
		{
			HashSizeInBits = 32
		});

		private static string[] wordDelimiters = new[]
		{
			" ",
			"&nbsp;",
			"\n"
		};



		public static Model Build(int id, string[] tags, string body)
		{
			var tagHashes = new int[tags.Length];

			for (var i = 0; i < tags.Length; i++)
			{
				var hashBytes = hash.ComputeHash(tags[i]).Hash;

				tagHashes[i] = BitConverter.ToInt32(hashBytes, 0);
			}

			var b = GetBodyTerms(body);

			return new Model
			{
				Id = id,
				Tags = tagHashes,
				Terms = null
			};
		}



		private static Dictionary<int, byte> GetBodyTerms(string body)
		{
			var tokens = TokenisePost(body);

			// clean up tokens, stem/remove punctuation/americanise

			return null;
		}

		public static List<PostToken> TokenisePost(string body)
		{
			var tokens = new List<PostToken>();
			var tagSplit = SplitTags(body);
			var layers = new List<WordType>();

			for (var i = 0; i < tagSplit.Count; i++)
			{
				if (tagSplit[i][0] == '<')
				{
					if (tagSplit[i].Length > 1 && tagSplit[i][1] == '/')
					{
						layers.RemoveAt(layers.Count - 1);

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
					.Split(wordDelimiters, StringSplitOptions.RemoveEmptyEntries)
					.ToList();
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
