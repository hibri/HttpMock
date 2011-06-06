using System;

namespace HttpMock
{
	public class HttpServerFactory
	{
		public static HttpServer BuildServer(Uri uri) {
			var httpServer = new HttpServer(uri);
			httpServer.Start();
			return httpServer;
		}
	}
}