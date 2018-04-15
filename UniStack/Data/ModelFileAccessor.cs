using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace UniStack.Data
{
	public static class ModelFileAccessor
	{
		private const string dataFile = "models.unistack";

		public static bool DataMissing => File.Exists(dataFile);



		public static object GetModel()
		{
			return null;
		}
	}
}
