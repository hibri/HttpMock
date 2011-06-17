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

		private IHttpServer BuildServer(Uri uri) {
			IHttpServer httpServer = _httpServerBuilder.Build(uri);
			httpServer.Start();
			return httpServer;
		}

		public IHttpServer Create(Uri uri) {
			if (!_httpServers.ContainsKey(uri.Port)) {
				IHttpServer httpServer = BuildServer(uri);
				_httpServers.Add(uri.Port, httpServer);
			}

			return _httpServers[uri.Port];
		}
	}
}