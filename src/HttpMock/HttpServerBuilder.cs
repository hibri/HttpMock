using System;

namespace HttpMock
{
	public class HttpServerBuilder : IHttpServerBuilder
	{
		public IHttpServer Build(Uri uri) {
			return new HttpServer(uri);
		}
	}
}