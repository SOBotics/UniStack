using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;

namespace UniStack.Data
{
	public class DataDumpParser
	{
		public class Post
		{
			public int Id;
			public string[] Tags;
			public string Body;
		}

		private const string dumpPathConfigKey = "DataDumpPath";
		private string dumpPath;
		private const string xmlQuestionPostType = "PostTypeId=\"1\"";
		private const string xmlPostId = "Id=\"";
		private const string xmlQuestionTags = "Tags=\"";
		private const string xmlPostBody = "Body=\"";

		public int CurrentQuestionsParsed { get; private set; }


		public DataDumpParser()
		{
			var path = ConfigAccessor.GetValue<string>(dumpPathConfigKey);

			if (string.IsNullOrEmpty(path))
			{
				throw new Exception($"'{dumpPathConfigKey}' (from config) cannot be null or empty.");
			}

			dumpPath = path;
		}



		public QuestionPool ParseTag(string tag)
		{
			CurrentQuestionsParsed = 0;

			tag = tag.Trim().ToLowerInvariant();
			var outputFile = Path.Combine(ModelFileAccessor.DataDir, tag);
			var qPool = new QuestionPool(1000);

			using (var inFs = File.Open(dumpPath, FileMode.Open))
			using (var outFs = File.Open(outputFile, FileMode.Create))
			using (var reader = new StreamReader(inFs))
			using (var writer = new StreamWriter(outFs))
			{
				string line;

				while ((line = reader.ReadLine()) != null)
				{
					var p = ParseLine(tag, line);

					if (p != null)
					{
						var m = ModelBuilder.Build(p.Id, p.Tags, p.Body);
						
						//TODO: write model to file

						//qPool.Add(m.Id, m.Tags, m.Terms);

						CurrentQuestionsParsed++;
					}
				}
			}

			return qPool;
		}

		private static Post ParseLine(string tag, string line)
		{
			if (!line.Contains(xmlQuestionPostType)) return null;

			var idStartIndex = line.IndexOf(xmlPostId, StringComparison.Ordinal) + xmlPostId.Length;
			var idLength = line.IndexOf('"', idStartIndex) - idStartIndex;
			var idStr = line.Substring(idStartIndex, idLength);
			var id = int.Parse(idStr);

			var tagsStartIndex = line.IndexOf(xmlQuestionTags, StringComparison.Ordinal) + xmlQuestionTags.Length;
			var tagsLength = line.IndexOf('"', tagsStartIndex) - tagsStartIndex;
			var tagsStr = HttpUtility.HtmlDecode(line.Substring(tagsStartIndex, tagsLength));
			var tags = tagsStr.Split(new[] { '<', '>' }, StringSplitOptions.RemoveEmptyEntries);

			var hasTag = false;
			foreach (var t in tags)
			{
				if (t == tag)
				{
					hasTag = true;

					break;
				}
			}

			if (!hasTag) return null;

			var bodyStartIndex = line.IndexOf(xmlPostBody, StringComparison.Ordinal) + xmlPostBody.Length;
			var bodyLength = line.IndexOf('"', bodyStartIndex) - bodyStartIndex;
			var body = HttpUtility.HtmlDecode(line.Substring(bodyStartIndex, bodyLength));

			return new Post
			{
				Id = id,
				Tags = tags,
				Body = body
			};
		}
	}
}
