using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Chat;

namespace UniStack.Chat.Commands
{
	public class Commands : ICommand
	{
		private Regex ptn = new Regex("(?i)^c(omman|m)ds?$", Extensions.GlobalRegexOptions);

		public string Name => "help";

		public string Description => "Prints this list of commands.";

		public Regex Pattern => ptn;



		public string Execute(Message msg)
		{
			var cmds = new HashSet<ICommand>(typeof(Commands).Assembly
				.GetTypes()
				.Where(x => !x.IsAbstract && x.IsClass)
				.Where(x => x
					.GetInterfaces()
					.Any(y => y.GetType() == typeof(ICommand))
				)
				.Select(Activator.CreateInstance)
				.Cast<ICommand>()
			);
			var sb = new StringBuilder();

			foreach (var cmd in cmds)
			{
				sb.Append(cmd.Name);
				sb.Append(" ~ ");
				sb.Append(cmd.Description);
				sb.Append("\n");
			}

			return sb.ToString().Trim();
		}
	}
}
