using System;
using System.Linq;
using System.Text;
using Kayak;

namespace HttpMock
{
    public class BufferedConsumer : IDataConsumer
	{
        readonly Action<string> _resultCallback;
        readonly Action<Exception> _errorCallback;
        private readonly StringBuilder _buffer;

        public BufferedConsumer(Action<string> resultCallback,
		                        Action<Exception> errorCallback)
		{
			_resultCallback = resultCallback;
			_errorCallback = errorCallback;

		    _buffer = new StringBuilder();
		}
		public bool OnData(ArraySegment<byte> data, Action continuation)
		{
		    _buffer.Append(Encoding.UTF8.GetString(data.Array, data.Offset, data.Count));

			return false;
		}
		public void OnError(Exception error)
		{
			_errorCallback(error);
		}

		public void OnEnd()
		{
            _resultCallback(_buffer.ToString());
		}
	}
}