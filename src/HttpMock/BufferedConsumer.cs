using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;

namespace HttpMock
{
	class BufferedConsumer : IDataConsumer
	{
		List<ArraySegment<byte>> buffer = new List<ArraySegment<byte>>();
		Action<string> resultCallback;
		Action<Exception> errorCallback;

		public BufferedConsumer(Action<string> resultCallback,
		                        Action<Exception> errorCallback)
		{
			this.resultCallback = resultCallback;
			this.errorCallback = errorCallback;
		}
		public bool OnData(ArraySegment<byte> data, Action continuation)
		{
			buffer.Add(data);
			return false;
		}
		public void OnError(Exception error)
		{
			errorCallback(error);
		}

		public void OnEnd()
		{
			var str = buffer
				.Select(b => Encoding.UTF8.GetString(b.Array, b.Offset, b.Count))
				.Aggregate((result, next) => result + next);

			resultCallback(str);
		}
	}
}