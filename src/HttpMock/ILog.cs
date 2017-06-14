using System;

namespace HttpMock
{
	public interface ILog
	{
		void Error(string message, Exception exception);

		void DebugFormat(string format, params object[] args);
	}
}