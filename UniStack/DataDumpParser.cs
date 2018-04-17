using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;

namespace UniStack
{
	public class DataDumpParser
	{
		private const string dumpPathConfigKey = "DataDumpPath";
		private string dumpPath;

		public DataDumpParser()
		{
			var path = ConfigAccessor.GetValue<string>(dumpPathConfigKey);

			if (string.IsNullOrEmpty(path))
			{
				throw new Exception($"'{dumpPathConfigKey}' (from config) cannot be null or empty.");
			}

			dumpPath = path;
		}

		public void Parse(string outputFile)
		{
			if (string.IsNullOrEmpty(outputFile))
			{
				throw new ArgumentException(nameof(outputFile));
			}

			using (var inFs = File.Open(dumpPath, FileMode.Create))
			using (var outFs = File.Open(outputFile, FileMode.Create))
			{

			}
		}
	}
}
