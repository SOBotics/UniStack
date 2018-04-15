using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Chat;

namespace UniStack.Chat
{
	public interface ICommand
	{
		string Name { get; }
		string Description { get; }
		Regex Pattern { get; }

		string Execute(Message cmd);
	}
}
