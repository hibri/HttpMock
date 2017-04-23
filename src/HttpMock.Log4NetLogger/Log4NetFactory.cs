using log4net;

namespace HttpMock.Log4NetLogger
{
	public static class Log4NetFactory
	{
		public static void UseLog4Net()
		{
			LogFactory.SetLoggerFactory(type => new Logger(LogManager.GetLogger(type)));
		}
	}
}
