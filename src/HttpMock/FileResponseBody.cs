using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Kayak;
using log4net;

namespace HttpMock
{
	class FileResponseBody : IDataProducer
	{
		private readonly string _filepath;
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public FileResponseBody(string filepath) {
			_filepath = filepath;
		}

		public IDisposable Connect(IDataConsumer channel) {
			var fileInfo = new FileInfo(_filepath);
			using(FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read)) {
				var buffer = new byte[fileInfo.Length];
				fileStream.Read(buffer, 0, (int) fileInfo.Length);
				channel.OnData(new ArraySegment<byte>(buffer), null);
				
				_log.DebugFormat("Wrote {0} bytes to buffer", fileInfo.Length);
				channel.OnEnd();
				return null;
			}
		}
	}
}