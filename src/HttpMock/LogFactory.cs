using System;

namespace HttpMock
{
	public static class LogFactory
	{
		private static Func<Type, ILog> _constructor;

		public static ILog GetLogger(Type type)
		{
			if (_constructor == null)
			{
				return new VoidLogger();
			}

			return _constructor.Invoke(type);
		}

		public static void SetLoggerFactory(Func<Type, ILog> constructor)
		{
			_constructor = constructor;
		}

		private class VoidLogger : ILog
		{
			public void Error(string message, Exception exception)
			{
			}

			public void DebugFormat(string format, params object[] args)
			{
			}
		}
	}
}