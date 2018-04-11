using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json.Linq;

namespace UniStack
{
	public static class ConfigAccessor
	{
		public static string ConfigFile { get; set; } = "config.json";

		public static T GetValue<T>(string path)
		{
			var token = GetToken(path);

			if (token == null)
			{
				return default(T);
			}

			return token.Value<T>();
		}



		private static JToken GetToken(string path)
		{
			var json = File.ReadAllText(ConfigFile);
			var obj = JObject.Parse(json);

			return obj.SelectToken(path);
		}
	}
}
