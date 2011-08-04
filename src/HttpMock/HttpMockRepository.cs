using System;

namespace HttpMock
{
	public static class HttpMockRepository
	{
		private static readonly HttpServerFactory _httpServerFactory = new HttpServerFactory(new HttpServerBuilder());

		public static IHttpServer At(Uri uri) {
			return _httpServerFactory.Create(uri).WithNewContext(uri.AbsolutePath);
		}

		public static IHttpServer At(string uri) {
			return At(new Uri(uri));
		}
	}
}