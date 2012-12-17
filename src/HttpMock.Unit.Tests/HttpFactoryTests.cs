
using System;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class HttpFactoryTests
	{
		[Test]
		[Ignore ("Checking if failure in TeamCity is related to the inclusion of this test")]
		public void ShouldBeAbleToHostAtSameAddressIfPreviousWasDisposed()
		{
			var serverFactory = new HttpServerFactory();

			var uri = new Uri(String.Format("http://localhost:{0}", PortHelper.FindLocalAvailablePortForTesting()));
			var server = serverFactory.Get(uri).WithNewContext(uri.AbsoluteUri);
			server.Start();
			server.Dispose();

			var httpServer2 = serverFactory.Get(uri).WithNewContext(uri.AbsoluteUri);
			Assert.DoesNotThrow(httpServer2.Start);
		}
	}
}
