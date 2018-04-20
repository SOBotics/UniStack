using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace UniStack.Data
{
	public static class ModelFileAccessor
	{
		public static string DataDir { get; } = "models";

		public static string[] AvailableModels
		{
			get
			{
				var models = Directory.EnumerateFiles(DataDir);

				return models.ToArray();
			}
		}



		static ModelFileAccessor()
		{
			if (!Directory.Exists(DataDir))
			{
				Directory.CreateDirectory(DataDir);
			}
		}



		public static bool TagHasModelFile(string tag)
		{
			var path = Path.Combine(DataDir, tag);

			return File.Exists(path);
		}

		//TODO: Add function for adding/updating models

		public static object GetModel()
		{
			return null;
		}
	}
}
