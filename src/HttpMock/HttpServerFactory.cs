using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	public class HttpServerFactory
	{
		private readonly object _serverLock = new object();
		private readonly Dictionary<int, IHttpServer> _httpServers = new Dictionary<int, IHttpServer>();

		public IHttpServer Get(Uri uri, ILoggerFactory loggerFactory = null)
		{
			lock (_serverLock)
			{
				if (_httpServers.TryGetValue(uri.Port, out var existing) && existing.IsAvailable())
					return existing;

				var server = BuildServer(uri, loggerFactory);
				_httpServers[uri.Port] = server;
				return server;
			}
		}

		public IHttpServer Create(Uri uri, ILoggerFactory loggerFactory = null)
		{
			var server = BuildServer(uri, loggerFactory);
			lock (_serverLock)
			{
				_httpServers[uri.Port] = server;
			}
			return server;
		}

		private static IHttpServer BuildServer(Uri uri, ILoggerFactory loggerFactory = null)
		{
			IHttpServer httpServer = new HttpServer(uri, loggerFactory);
			httpServer.Start();
			return httpServer;
		}
	}
}