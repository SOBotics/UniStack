using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Chat;

namespace UniStack
{
	public static class Extensions
	{
		private static readonly Regex pingRemover = new Regex("@\\S{2,}\\s?", GlobalRegexOptions);

		public static RegexOptions GlobalRegexOptions => RegexOptions.Compiled | RegexOptions.CultureInvariant;

		public static string GetCleanText(this Message msg) =>
			pingRemover.Replace(msg?.Text ?? "", "").Trim();

		public static int ToInt(this byte[] bytes) =>
			bytes[0] << 24 ^
			bytes[1] << 16 ^
			bytes[2] << 8 ^
			bytes[3];
	}
}
