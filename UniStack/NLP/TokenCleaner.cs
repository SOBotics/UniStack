using System;
using System.Collections.Generic;
using System.Text;

namespace UniStack.NLP
{
	public static class TokenCleaner
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



		public static List<string> ExpandContraction(PostTokeniser.PostToken token)
		{
			if (token.Type != PostTokeniser.WordType.Text)
			{
				return new List<string> { token.Word };
			}

			var split = token.Word.Split('\'');
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
							words.Add(split[0]);
							words.Add("not");
						}

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
						words.Add(token.Word);
						break;
					}
				}

				return words;
			}
			else
			{
				var wordLower = token.Word.ToLowerInvariant();

				if (commonIncorrectContractions.ContainsKey(wordLower))
				{
					words.AddRange(commonIncorrectContractions[wordLower]);
				}
				else if (wordLower == "im")
				{
					if (token.Word[1] == 'm')
					{
						words.Add("I");
						words.Add("am");
					}
					else
					{
						words.Add(token.Word);
					}
				}
				else
				{
					words.Add(token.Word);
				}

				return words;
			}
		}
	}
}