using System;
using System.Collections.Generic;

namespace HttpMock
{
	public class HttpServerFactory
	{
		private readonly IHttpServerBuilder _httpServerBuilder;
		private readonly Dictionary<int, IHttpServer> _httpServers = new Dictionary<int, IHttpServer>();

		public HttpServerFactory(IHttpServerBuilder httpServerBuilder) {
			_httpServerBuilder = httpServerBuilder;
		}

		public IHttpServer Get(Uri uri)
		{
			if (_httpServers.ContainsKey(uri.Port))
			{
				IHttpServer httpServer = _httpServers[uri.Port];
				if (!httpServer.IsAvailable()) throw new InvalidOperationException("Socket has not been released!");
				return httpServer;
			}

			return Create(uri);
		}

		public IHttpServer Create(Uri uri) {
			IHttpServer httpServer = BuildServer(uri);
			_httpServers.Add(uri.Port, httpServer);

			return _httpServers[uri.Port];
		}

		private IHttpServer BuildServer(Uri uri) {
			IHttpServer httpServer = _httpServerBuilder.Build(uri);
			httpServer.Start();
			return httpServer;
		}
	}
}