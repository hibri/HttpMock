using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HttpMock
{
	public static class HttpMockLogging
	{
		private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

		public static void Configure(ILoggerFactory loggerFactory)
		{
			_loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
		}

		internal static ILogger<T> CreateLogger<T>()
		{
			return _loggerFactory.CreateLogger<T>();
		}
	}
}
