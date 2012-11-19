using System;

namespace HttpMock
{
	public static class HttpMockRepository
	{
		private static readonly HttpServerFactory _httpServerFactory = new HttpServerFactory();

		public static IHttpServer At(string uri) {
			if (uri.Trim ().EndsWith ("/")) {
				throw new ArgumentException (
					String.Format ("Do not use a trailing slash for the server URI please: {0}", uri), "uri");
			}
			return At(new Uri(uri));
		}

		public static IHttpServer At(Uri uri)
		{
			return _httpServerFactory.Get(uri).WithNewContext(uri.AbsolutePath);
		}
	}
}