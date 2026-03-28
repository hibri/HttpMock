using System;
using System.Collections.Generic;
using System.Text;

namespace HttpMock
{
	class BufferedBody : IResponse
	{
		private readonly Func<byte[]> _dataFunc;

		public BufferedBody(string data) : this(data, Encoding.UTF8) { }
		public BufferedBody(string data, Encoding encoding) : this(encoding.GetBytes(data)) { }
		public BufferedBody(byte[] data) : this(() => data) { }
		public BufferedBody(Func<string> data) : this(() => Encoding.UTF8.GetBytes(data())) { }
		public BufferedBody(Func<byte[]> data)
		{
			_dataFunc = data;
		}

		public Func<int> Length => () => _dataFunc().Length;

		public byte[] GetBytes()
		{
			return _dataFunc();
		}

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders) { }
	}

	class NoBody : IResponse
	{
		public byte[] GetBytes() => new byte[0];

		public void SetRequestHeaders(IDictionary<string, string> requestHeaders) { }
	}
}