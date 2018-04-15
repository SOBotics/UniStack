using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Chat;

namespace UniStack.Chat.Commands
{
	public class Help : ICommand
	{
		private Regex ptn = new Regex("(?i)^h[ea]lp[!?]*$", Extensions.GlobalRegexOptions);

		public string Name => "help";

		public string Description => "Prints basic info on what the bot does.";

		public Regex Pattern => ptn;



		public string Execute(Message msg)
		{
			var repoLink = "https://github.com/SOBotics/UniStack";

			return
				"I'm a chatbot that scans Stack Overflow in search of " +
				$"duplicate questions. You can find my repository [here]({repoLink}).";
		}
	}
}
