
using System;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class HttpFactoryTests
	{
		[Test]
		[Ignore ("Makes the test suite flaky (???)")]
		public void ShouldBeAbleToHostAtSameAddressIfPreviousWasDisposed()
		{
			var serverFactory = new HttpServerFactory();

			var uri = new Uri(String.Format("http://localhost:{0}", PortHelper.FindLocalAvailablePortForTesting()));
			IHttpServer server1, server2 = null;
			server1 = serverFactory.Get(uri).WithNewContext(uri.AbsoluteUri);
			try
			{
				server1.Start();
				server1.Dispose();
				server1 = null;

				server2 = serverFactory.Get(uri).WithNewContext(uri.AbsoluteUri);
				Assert.DoesNotThrow(server2.Start);
			}
			finally
			{
				if (server1 != null)
				{
					server1.Dispose();
				}
				if (server2 != null)
				{
					server2.Dispose();
				}
			}
		}
	}
}
