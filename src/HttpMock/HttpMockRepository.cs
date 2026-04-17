using System;
using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	public static class HttpMockRepository
	{
		private static readonly HttpServerFactory _httpServerFactory = new HttpServerFactory();

		public static IHttpServer At(string uri, ILoggerFactory loggerFactory = null)
		{
			if (uri.Trim().EndsWith("/"))
			{
				throw new ArgumentException(
					String.Format("Do not use a trailing slash for the server URI please: {0}", uri), "uri");
			}

			return At(new Uri(uri), loggerFactory);
		}

		public static IHttpServer At(Uri uri, ILoggerFactory loggerFactory = null)
		{
			return _httpServerFactory.Get(uri, loggerFactory).WithNewContext();
		}

		/// <summary>
		/// Asks the operating system for a free TCP port on the loopback interface.
		/// </summary>
		/// <remarks>
		/// The port is reserved by <see cref="TcpListener"/> on port 0 and released
		/// immediately. There is a small TOCTOU window between the release and the
		/// caller binding to the port; this is an accepted limitation for test/mock usage.
		/// </remarks>
		/// <returns>An available port number.</returns>
		public static int FindFreePort()
		{
			using var listener = new TcpListener(IPAddress.Loopback, 0);
			listener.Start();
			var port = ((IPEndPoint)listener.LocalEndpoint).Port;
			listener.Stop();
			return port;
		}
	}
}
