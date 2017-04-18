using System;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpFactoryTests
	{
		[Test]
		public void ShouldBeAbleToHostAtSameAddressIfPreviousWasDisposed()
		{
			var serverFactory = new HttpServerFactory();

			var uri = new Uri(HostHelper.GenerateAHostUrlForAStubServer());
			IHttpServer server1, server2 = null;
			server1 = serverFactory.Get(uri).WithNewContext();
			try
			{
				server1.Start();
				server1.Dispose();
				server1 = null;

				server2 = serverFactory.Get(uri).WithNewContext();
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
