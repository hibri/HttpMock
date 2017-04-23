using System;

namespace HttpMock.Log4NetLogger
{
	public class Logger : ILog
	{
		private readonly log4net.ILog _log;

		public Logger(log4net.ILog log)
		{
			_log = log;
		}

		public void Error(string message, Exception exception)
		{
			if (_log.IsErrorEnabled)
			{
				_log.Error(message, exception);
			}
		}

		public void DebugFormat(string format, params object[] args)
		{
			if (_log.IsDebugEnabled)
			{
				_log.DebugFormat(format, args);
			}
		}
	}
}