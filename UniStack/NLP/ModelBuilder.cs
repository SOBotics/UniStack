using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Data.HashFunction.xxHash;
using System.IO;
using System.Text;
using NFastTag;

namespace UniStack.NLP
{
	public static class ModelBuilder
	{
		public class Model
		{
			public int Id;
			public int[] Tags;
			public Dictionary<int, byte> Terms;
		}

		private static IxxHash hash;
		private static FastTag tagger;



		static ModelBuilder()
		{
			var lexicon = File.ReadAllText("lexicon.txt");

			tagger = new FastTag(lexicon);

			hash = xxHashFactory.Instance.Create(new xxHashConfig
			{
				HashSizeInBits = 32
			});
		}



		public static Model Build(int id, string[] tags, string body)
		{
			var tagHashes = new int[tags.Length];

			for (var i = 0; i < tags.Length; i++)
			{
				var hashBytes = hash.ComputeHash(tags[i]).Hash;

				tagHashes[i] = BitConverter.ToInt32(hashBytes, 0);
			}

			var terms = GetBodyTerms(body);

			return new Model
			{
				Id = id,
				Tags = tagHashes,
				Terms = terms
			};
		}



		private static Dictionary<int, byte> GetBodyTerms(string body)
		{
			var tokens = PostTokeniser.Tokenise(body);
			var words = GetWords(tokens);
			var tagged = tagger.Tag(words);
			var ngrams = new Dictionary<string, byte>();

			for (var i = 0; i < tagged.Count; i++)
			{
				var ngram = GetNGram(tagged, i, 1);
				//var ngramHash = hash.ComputeHash(ngram).Hash.ToInt();

				if (ngrams.ContainsKey(ngram))
				{
					if (ngrams[ngram] == byte.MaxValue) continue;

					ngrams[ngram]++;
				}
				else
				{
					ngrams[ngram] = 1;
				}
			}

			Console.WriteLine("");
			foreach (var x in ngrams)
			{
				Console.WriteLine($"{x.Value} {x.Key}");
			}

			return null;
		}

		private static List<string> GetWords(List<PostTokeniser.PostToken> tokens)
		{
			var words = new List<string>();

			foreach (var t in tokens)
			{
				if (t.Type == PostTokeniser.WordType.CodeBlock ||
					t.Type == PostTokeniser.WordType.Unknown ||
					t.Type == PostTokeniser.WordType.None)
				{
					continue;
				}

				var apIndex = t.Word.IndexOf('\'');

				if (apIndex != -1)
				{
					if (apIndex != t.Word.Length - 2)
					{
						continue;
					}
					else
					{
						var lastApIndex = t.Word.LastIndexOf('\'');

						if (lastApIndex != t.Word.Length - 2)
						{
							continue;
						}
					}
				}

				var builder = new StringBuilder();

				for (var i = 0; i < t.Word.Length; i++)
				{
					var c = t.Word[i];

					if (char.IsLetter(c) || c == '\'')
					{
						builder.Append(char.ToLowerInvariant(c));
					}
				}

				var w = builder.ToString();

				if (w.Length == 0) continue;

				if (Americaniser.Americanise(w, out var americanised))
				{
					words.Add(americanised);
				}
				else
				{
					var ws = ContractionExpander.Expand(w);

					words.AddRange(ws);
				}
			}

			return words;
		}

		private static string GetNGram(List<FastTagResult> tags, int index, int wordCount)
		{
			var ngram = new StringBuilder();

			for (var i = index; i < index + wordCount; i++)
			{
				ngram.Append(tags[i].Word);
				ngram.Append("_");
				ngram.Append(tags[i].PosTag);
				ngram.Append(" ");
			}

			return ngram.ToString().Trim();
		}
	}
}
