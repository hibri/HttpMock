using System;
using System.IO;
using System.Text;
using System.Threading;
using Kayak;

namespace HttpMock
{
	class BufferedBody : IDataProducer
	{
		ArraySegment<byte> data;

		public BufferedBody(string data) : this(data, Encoding.UTF8) { }
		public BufferedBody(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }
		public BufferedBody(byte[] data) : this(new ArraySegment<byte>(data)) { }
		public BufferedBody(ArraySegment<byte> data)
		{
			this.data = data;
		}

		public IDisposable Connect(IDataConsumer channel)
		{
			// null continuation, consumer must swallow the data immediately.
			channel.OnData(data, null);
			channel.OnEnd();
			return null;
		}
	}

	class NoBody : IDataProducer
	{
		public IDisposable Connect(IDataConsumer channel) {
			return null;
		}
	}

	class FileResponseBody : IDataProducer
	{
		private readonly string _filepath;
		const int BUFFER_SIZE = 2048;
		public FileResponseBody(string filepath) {
			_filepath = filepath;
		}

		public IDisposable Connect(IDataConsumer channel) {

			var fileInfo = new FileInfo(_filepath);
			
			using(FileStream fileStream = fileInfo.Open(FileMode.Open, FileAccess.Read)) {
				var buffer = new byte[fileInfo.Length];
				fileStream.Read(buffer, 0, BUFFER_SIZE);
				channel.OnData(new ArraySegment<byte>(buffer), null);
				return null;
			}
		}
	}

	class StreamedBody : IDataProducer
	{
		private readonly Stream _stream;
		private readonly int _contentLength;
		private IDataConsumer _channel;
		const int BUFFER_SIZE = 2048;
		public StreamedBody(Stream stream, int contentLength) {
			_stream = stream;
			_contentLength = contentLength;
		}

		public IDisposable Connect(IDataConsumer channel) {
			_channel = channel;

			using (_stream) {
				byte[] buffer = new byte[_contentLength];
				_stream.Read(buffer, 0, BUFFER_SIZE);

				_channel.OnData(new ArraySegment<byte>(buffer), null);
				return null;
			}
		}
	}
}