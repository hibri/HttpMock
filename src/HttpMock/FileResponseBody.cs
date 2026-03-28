using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace HttpMock
{
	class FileResponseBody : IResponse
	{
		private readonly string _filepath;
		private static readonly ILog _log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private IDictionary<string, string> _requestHeaders;

		public FileResponseBody(string filepath) {
			_filepath = filepath;
		}

		public byte[] GetBytes()
		{
			var fileInfo = new FileInfo(_filepath);
			using (FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read))
			{
				var buffer = new byte[fileInfo.Length];
				fileStream.Read(buffer, 0, (int)fileInfo.Length);
				int length = (int)fileInfo.Length;
				int offset = 0;

				if (_requestHeaders != null && _requestHeaders.ContainsKey(HttpRequestHeader.Range.ToString()))
				{
					string range = _requestHeaders[HttpRequestHeader.Range.ToString()];
					Regex rangeEx = new Regex(@"bytes=([\d]*)-([\d]*)");
					if (rangeEx.IsMatch(range))
					{
						int from = Convert.ToInt32(rangeEx.Match(range).Groups[1].Value);
						int to = Convert.ToInt32(rangeEx.Match(range).Groups[2].Value);
						offset = from;
						length = (to - from) + 1;
					}
				}

				var result = new byte[length];
				Array.Copy(buffer, offset, result, 0, length);
				_log.DebugFormat("Wrote {0} bytes to buffer", length);
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