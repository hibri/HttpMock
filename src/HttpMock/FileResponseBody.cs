using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	class FileResponseBody : IResponse
	{
		private static readonly Regex RangeRegex = new(@"bytes=([\d]*)-([\d]*)", RegexOptions.Compiled);
		private readonly string _filepath;
		private readonly ILogger<FileResponseBody> _log;
		private IDictionary<string, string> _requestHeaders;

		public FileResponseBody(string filepath) {
			_filepath = filepath;
			_log = HttpMockLogging.CreateLogger<FileResponseBody>();
		}

		public byte[] GetBytes()
		{
			var fileInfo = new FileInfo(_filepath);
			using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read))
			{
				int length = (int)fileInfo.Length;
				int offset = 0;

				if (_requestHeaders != null && _requestHeaders.ContainsKey(HttpRequestHeader.Range.ToString()))
				{
					string range = _requestHeaders[HttpRequestHeader.Range.ToString()];
					var match = RangeRegex.Match(range);
					if (match.Success)
					{
						int from = Convert.ToInt32(match.Groups[1].Value);
						int to = Convert.ToInt32(match.Groups[2].Value);
						offset = from;
						length = (to - from) + 1;
					}
				}

				if (offset > 0)
				{
					fileStream.Seek(offset, SeekOrigin.Begin);
				}

				var result = new byte[length];
				fileStream.ReadExactly(result, 0, length);
				_log.LogDebug("Wrote {Length} bytes to buffer", length);
				return result;
			}
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders) {
			_requestHeaders = requestHeaders;
		}
	}

	internal interface IResponse
	{
		byte[] GetBytes();
		void SetRequestHeaders(IDictionary<string, string> requestHeaders);
	}
}