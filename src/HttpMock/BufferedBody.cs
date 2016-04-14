using System;
using System.Collections.Generic;
using System.Text;
using Kayak;

namespace HttpMock
{
	class BufferedBody : IResponse
	{
		readonly Func<byte[]> dataFunc;

		public BufferedBody(string data) : this(data, Encoding.UTF8) { }
		public BufferedBody(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }
		public BufferedBody(byte[] data) : this(() => data) { }
		public BufferedBody(Func<string> data) : this(() => Encoding.UTF8.GetBytes(data())) { }
		public BufferedBody(Func<byte[]> data)
		{
			this.dataFunc = data;
		}

		public Func<int> Length => () => dataFunc().Length;

		public IDisposable Connect(IDataConsumer channel)
		{
			// null continuation, consumer must swallow the data immediately.
			var bytes = new ArraySegment<byte>(dataFunc());
			channel.OnData(bytes, null);
			channel.OnEnd();
			return null;
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders) {
		}
	}

	class NoBody : IResponse
	{
		public IDisposable Connect(IDataConsumer channel) {
			return null;
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders) {
		}
	}
}