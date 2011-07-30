using System;
using Kayak;

namespace HttpMock
{
	public class NoBody : IDataProducer
	{
		public IDisposable Connect(IDataConsumer channel) {
			return null;
		}
	}
}