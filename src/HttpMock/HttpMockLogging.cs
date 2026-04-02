using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace HttpMock
{
	/// <summary>
	/// Optional global logging configuration for HttpMock.
	/// Prefer passing <see cref="ILoggerFactory"/> directly to <see cref="HttpServer"/> for
	/// standard .NET constructor-injection-style logging.  This class exists as a convenience
	/// for scenarios where direct constructor injection is not possible.
	/// </summary>
	public static class HttpMockLogging
	{
		private static ILoggerFactory _loggerFactory = NullLoggerFactory.Instance;

		public static void Configure(ILoggerFactory loggerFactory)
		{
			Volatile.Write(ref _loggerFactory, loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory)));
		}

		internal static ILogger<T> CreateLogger<T>()
		{
			return Volatile.Read(ref _loggerFactory).CreateLogger<T>();
		}

		internal static ILoggerFactory GetLoggerFactory()
		{
			return Volatile.Read(ref _loggerFactory);
		}
	}
}
