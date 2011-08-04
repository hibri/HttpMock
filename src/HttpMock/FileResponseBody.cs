using System;
using System.IO;
using Kayak;

namespace HttpMock
{
	class FileResponseBody : IDataProducer
	{
		private readonly string _filepath;

		public FileResponseBody(string filepath) {
			_filepath = filepath;
		}

		public IDisposable Connect(IDataConsumer channel) {
			var fileInfo = new FileInfo(_filepath);
			
			using(FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read)) {
				var buffer = new byte[fileInfo.Length];
				fileStream.Read(buffer, 0, (int) fileInfo.Length);
				channel.OnData(new ArraySegment<byte>(buffer), null);
				return null;
			}
		}
	}
}