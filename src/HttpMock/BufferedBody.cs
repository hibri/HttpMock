using System;
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
}