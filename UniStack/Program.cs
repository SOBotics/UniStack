﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using StackExchange.Auth;
using StackExchange.Chat.Actions;
using StackExchange.Chat.Events;
using StackExchange.Net.WebSockets;
using UniStack.Data;

namespace UniStack
{
	public class Program
	{
		private static CommandRouter commandsHanlder;
		private static ActionScheduler chatWriter;
		private static RoomWatcher<DefaultWebSocket> roomWatcher;

		public static void Main(string[] args)
		{
			JoinRoom();

			if (ModelFileAccessor.AvailableModels.Length == 0)
			{
				InitialiseFromDataDump();
			}
			else
			{
				LoadModels();
			}

			WaitForExit();
		}



		private static void WaitForExit()
		{
			var mre = new ManualResetEvent(false);

			Console.CancelKeyPress += (o, e) => mre.Set();

			mre.WaitOne();

			Console.WriteLine("\n\nStopping...");

			chatWriter.CreateMessage("Shutting down.");
		}

		private static void JoinRoom()
		{
			Console.Write("Joining chat room...");

			var host = ConfigAccessor.GetValue<string>("Host");
			var email = ConfigAccessor.GetValue<string>("Email");
			var password = ConfigAccessor.GetValue<string>("Password");
			var auth = new EmailAuthenticationProvider(email, password);
			var cookies = auth.GetAuthCookies(host);

			var roomUrl = ConfigAccessor.GetValue<string>("RoomUrl");
			roomWatcher = new RoomWatcher<DefaultWebSocket>(cookies, roomUrl);
			chatWriter = new ActionScheduler(cookies, roomUrl);
			commandsHanlder = new CommandRouter(roomWatcher, chatWriter);

			chatWriter.CreateMessage("UniStack started.");

			Console.Write("done");
		}

		private static void InitialiseFromDataDump()
		{
			var startTxt = "Model files missing, initialising from data dump (this could take a while)...";
			Console.Write($"\n{startTxt}");
			chatWriter.CreateMessage(startTxt);

			var dumpPath = ConfigAccessor.GetValue<string>("DataDumpPath");
			var parser = new DataDumpParser();
			var tags = ConfigAccessor.GetValues<string>("Tags");

			foreach (var tag in tags)
			{
				Console.Write($"\nAdding {tag}...");
				chatWriter.CreateMessage($"Adding [tag:{tag}]...");

				Task.Run(() =>
				{
					Thread.Sleep(60 * 1000);

					while (parser.CurrentQuestionsParsed > 0)
					{
						var c = parser.CurrentQuestionsParsed.ToString("N0");

						chatWriter.CreateMessage($"Models: {c}");

						Thread.Sleep(60 * 1000);
					}
				});

				var sw = Stopwatch.StartNew();

				//TODO: do something with the returned question pool
				var qPool = parser.ParseTag(tag);

				sw.Stop();

				var qCount = /*qPool.Count*/parser.CurrentQuestionsParsed.ToString("N0");
				var time = sw.Elapsed.ToString("mm\\:ss");

				Console.Write($"done. {qCount} in {time}.");
				chatWriter.CreateMessage($"[tag:{tag}] added. {qCount} questions were parsed in {time}.");
			}
		}

		private static void LoadModels()
		{
			// 17mil Qs (25 terms + 3 tags) = 2.4GM ram

			//var qCount = 2133617;
			//var startTxt = $"\nLoading {qCount.ToString("N0")} questions into memory...";

			//Console.Write(startTxt);
			//chatWriter.CreateMessage(startTxt);

			//var qp = new QuestionPool(qCount);
			//var sw = Stopwatch.StartNew();

			//for (var i = 0; i < qCount; i++)
			//{
			//	var t = new Dictionary<int, byte>();
			//	var tCount = 50;// (DateTime.UtcNow.Ticks % 20) + 6;

			//	for (var j = 0; j < tCount; j++)
			//	{
			//		t[j] = (byte)j;
			//	}

			//	qp.Add(i, new[] { 1337, int.MaxValue, int.MinValue }, t);
			//}

			//sw.Stop();

			//var cMb = Math.Round(qp.Size / 1024.0 / 1024, 2).ToString("N0");
			//var avg = Math.Round(qp.Size * 1.0 / qCount).ToString("N0");
			//var finishTxt = $"\nLoad completed in {sw.ElapsedMilliseconds.ToString("N0")}ms, consuming {cMb}MiB ({avg} bytes/question).";

			//Console.WriteLine(finishTxt);
			//chatWriter.CreateMessage(finishTxt);
		}
	}
}
