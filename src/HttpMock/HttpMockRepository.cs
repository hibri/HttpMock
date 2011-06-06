using System;
using System.Collections.Generic;
using Kayak;

namespace HttpMock
{
	public static class HttpMockRepository 
	{
		private static readonly Dictionary<int, HttpServer> _httpServers = new Dictionary<int, HttpServer>();

		public static IStubHttp At(Uri uri)
		{
			if (!_httpServers.ContainsKey(uri.Port)) {
				HttpServer httpServer = HttpServerFactory.BuildServer(uri);
				_httpServers.Add(uri.Port, httpServer);
			}
			return _httpServers[uri.Port].WithNewContext(uri.AbsolutePath);
		}

		public static IStubHttp At(string uri)
		{
			return At(new Uri(uri));
		}
	}
}