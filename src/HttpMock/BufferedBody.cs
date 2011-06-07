using System;
using System.IO;
using System.Text;
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

	class StreamedBody : IDataProducer
	{
		private readonly Stream _stream;
		private readonly int _contentLength;
		private int _bytesSent;
		private IDataConsumer _channel;
		const int BUFFER_SIZE = 2048;
		public StreamedBody(Stream stream, int contentLength) {
			_stream = stream;
			_contentLength = contentLength;
		}

		public IDisposable Connect(IDataConsumer channel) {
			_bytesSent = 0;
			_channel = channel;

			using (_stream) {
				byte[] buffer = new byte[_contentLength];
				_bytesSent = _stream.Read(buffer, 0, BUFFER_SIZE);

				_channel.OnData(new ArraySegment<byte>(buffer), null);
				return null;
			}
		}

		private void WriteData() {
			
			if(_bytesSent <= _contentLength) {
				
				
			}
		}
	}
}