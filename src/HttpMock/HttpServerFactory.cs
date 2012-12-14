using System;
using System.Collections.Generic;

namespace HttpMock
{
	public class HttpServerFactory
	{
		private readonly Dictionary<int, IHttpServer> _httpServers = new Dictionary<int, IHttpServer>();

		public IHttpServer Get(Uri uri)
		{
			if (_httpServers.ContainsKey(uri.Port))
			{
				IHttpServer httpServer = _httpServers[uri.Port];
				if (httpServer.IsAvailable())
					return httpServer;
			}

			return Create(uri);
		}

		public IHttpServer Create(Uri uri) {
			IHttpServer httpServer = BuildServer(uri);
			_httpServers[uri.Port] = httpServer;
			return httpServer;
		}

		private IHttpServer BuildServer(Uri uri) {
			IHttpServer httpServer = new HttpServer(uri);
			httpServer.Start();
			return httpServer;
		}
	}
}