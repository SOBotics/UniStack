using System;
using System.Collections.Generic;
using System.Text;

namespace UniStack.NLP
{
	public static class ContractionExpander
	{
		private static Dictionary<string, string[]> commonIncorrectContractions = new Dictionary<string, string[]>
		{
			["aint"]     = new[] { "is",     "not" },
			["arent"]    = new[] { "are",    "not" },
			["cant"]     = new[] { "cannot" },
			["couldnt"]  = new[] { "could",  "not" },
			["didnt"]    = new[] { "did",    "not" },
			["doesnt"]   = new[] { "does",   "not" },
			["dont"]     = new[] { "do",     "not" },
			["hadnt"]    = new[] { "had",    "not" },
			["hasnt"]    = new[] { "has",    "not" },
			["havent"]   = new[] { "have",   "not" },
			["isnt"]     = new[] { "is",     "not" },
			["shouldnt"] = new[] { "should", "not" },
			["wasnt"]    = new[] { "was",    "not" },
			["werent"]   = new[] { "were",   "not" },
			["wont"]     = new[] { "will",   "not" },
			["wouldnt"]  = new[] { "would",  "not" },
			["youre"]    = new[] { "you",    "are" },
			["youve"]    = new[] { "you",    "have" }
		};



		public static List<string> Expand(string word)
		{
			var split = word.Split('\'');
			var words = new List<string>();

			if (split.Length == 2)
			{
				var cont = split[1].ToLowerInvariant();

				switch (cont)
				{
					case "t":
					{
						if (split[0].ToLowerInvariant() == "can")
						{
							words.Add("cannot");
						}
						else if (split[0].ToLowerInvariant() == "won")
						{
							words.Add("will");
							words.Add("not");
						}
						else if (split[0].ToLowerInvariant() == "ain")
						{
							// Yes, there are multiple meanings for this contraction;
							// this currently seems to be the most popular one.
							// Implementing a function that correctly deals with these
							// cases is simply not worth any gained accuracy.
							words.Add("is");
							words.Add("not");
						}
						else if (split[0].ToLowerInvariant() == "shan")
						{
							words.Add("shall");
							words.Add("not");
						}
						else
						{
							words.Add(split[0].Substring(0, split[0].Length - 1));
							words.Add("not");
						}

						break;
					}
					case "s":
					{
						// Not totally accurate since 's can be possessive.
						words.Add(split[0]);
						words.Add("is");
						break;
					}
					case "d":
					{
						// Not totally accurate since 'd can expand to
						// different words depending on context.
						words.Add(split[0]);
						words.Add("would");
						break;
					}
					case "m":
					{
						words.Add(split[0]);
						words.Add("am");
						break;
					}
					case "re":
					{
						words.Add(split[0]);
						words.Add("are");
						break;
					}
					case "ve":
					{
						words.Add(split[0]);
						words.Add("have");
						break;
					}
					case "ll":
					{
						words.Add(split[0]);
						words.Add("will");
						break;
					}
					default:
					{
						words.Add(word);
						break;
					}
				}

				return words;
			}
			else
			{
				var wordLower = word.ToLowerInvariant();

				if (commonIncorrectContractions.ContainsKey(wordLower))
				{
					words.AddRange(commonIncorrectContractions[wordLower]);
				}
				else if (wordLower == "im")
				{
					if (word[1] == 'm')
					{
						words.Add("I");
						words.Add("am");
					}
					else
					{
						words.Add(word);
					}
				}
				else
				{
					words.Add(word);
				}

				return words;
			}
		}
	}
}