using System;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	public static class HttpMockRepository
	{
		private static readonly HttpServerFactory _httpServerFactory = new HttpServerFactory();

		public static IHttpServer At(string uri, ILoggerFactory loggerFactory = null) {
			if (uri.Trim().EndsWith("/")) {
				throw new ArgumentException(
					String.Format("Do not use a trailing slash for the server URI please: {0}", uri), "uri");
			}
			return At(new Uri(uri), loggerFactory);
		}

		public static IHttpServer At(Uri uri, ILoggerFactory loggerFactory = null)
		{
			return _httpServerFactory.Get(uri, loggerFactory).WithNewContext();
		}
	}
}