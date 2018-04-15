using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using StackExchange.Chat;
using StackExchange.Chat.Actions;
using StackExchange.Chat.Events;
using StackExchange.Chat.Events.User.Extensions;
using StackExchange.Net.WebSockets;
using UniStack.Chat;

namespace UniStack
{
	public class CommandRouter
	{
		private readonly ActionScheduler scheduler;
		private readonly RoomWatcher<DefaultWebSocket> watcher;
		private readonly HashSet<ICommand> commands;



		public CommandRouter(RoomWatcher<DefaultWebSocket> roomWatcher, ActionScheduler actionScheduler)
		{
			scheduler = actionScheduler ?? throw new ArgumentNullException(nameof(actionScheduler));
			watcher = roomWatcher ?? throw new ArgumentNullException(nameof(roomWatcher));

			commands = new HashSet<ICommand>(typeof(CommandRouter).Assembly
				.GetTypes()
				.Where(x => !x.IsAbstract && x.IsClass)
				.Where(x => x
					.GetInterfaces()
					.Any(y => y.GetType() == typeof(ICommand))
				)
				.Select(Activator.CreateInstance)
				.Cast<ICommand>()
			);

			watcher.AddUserMentionedEventHandler(HandleNewPing);
		}



		private void HandleNewPing(Message msg)
		{
			var msgText = msg.GetCleanText();

			foreach (var command in commands)
			{
				if (command.Pattern.IsMatch(msgText))
				{
					var returnMsg = command.Execute(msg);

					if (!string.IsNullOrEmpty(returnMsg))
					{
						scheduler.CreateReply(returnMsg, msg);
					}
				}
			}
		}
	}
}
