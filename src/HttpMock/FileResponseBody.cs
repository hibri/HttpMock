using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Kayak;
using System.Diagnostics;

namespace HttpMock
{
	class FileResponseBody :  IResponse
	{
		private readonly string _filepath;
		private IDictionary<string, string> _requestHeaders;

		public FileResponseBody(string filepath) {
			_filepath = filepath;
		}

		public IDisposable Connect(IDataConsumer channel) {
			var fileInfo = new FileInfo(_filepath);
			using(FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read)) {
				var buffer = new byte[fileInfo.Length];
				fileStream.Read(buffer, 0, (int) fileInfo.Length);
				int length = (int) fileInfo.Length;
				int offset = 0;

				if(_requestHeaders.ContainsKey(HttpRequestHeader.Range.ToString())) {
					string range = _requestHeaders[HttpRequestHeader.Range.ToString()];
					Regex rangeEx = new Regex(@"bytes=([\d]*)-([\d]*)");
					if(rangeEx.IsMatch(range)) {
						int from = Convert.ToInt32(rangeEx.Match(range).Groups[1].Value);
						int to = Convert.ToInt32(rangeEx.Match(range).Groups[2].Value);
						offset = from;
						length = (to - from) +1;
					}
				}
				ArraySegment<byte> data = new ArraySegment<byte>(buffer, offset, length);
				channel.OnData(data, null);
				
                Debug.WriteLine("Wrote {0} bytes to buffer", data.Array.Length);
				channel.OnEnd();
				return null;
			}
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders) {
			_requestHeaders = requestHeaders;
		}
	}

	internal interface IResponse : IDataProducer {
		void SetRequestHeaders(IDictionary<string, string> requestHeaders);
	}
}