using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniStack.NLP
{
	// Based on https://github.com/stanfordnlp/CoreNLP/blob/master/src/edu/stanford/nlp/process/Americanize.java
	public static class Americaniser
	{
		private const string defsFile = "americaniser-definitions";
		private static Dictionary<string, string> words;

		static Americaniser()
		{
			words = File.ReadAllLines(defsFile)
				.Select(x => x.Split(' ', StringSplitOptions.RemoveEmptyEntries))
				.Where(x => x.Length == 2)
				.ToDictionary(x => x[0], x => x[1]);
		}

		public static string Americanise(string word)
		{
			var wordLower = word.ToLowerInvariant();

			if (words.ContainsKey(wordLower))
			{
				return words[wordLower];
			}

			return word;
		}

	}
}
