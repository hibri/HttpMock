using System;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpServerTests
	{
		[Test]
		public void IsAvailableReturnsFalseIfStartNotCalled()
		{
			IHttpServer httpServer = new HttpServer(new Uri(String.Format("http://localhost:{0}",
															              PortHelper.FindLocalAvailablePortForTesting())));
			Assert.That(httpServer.IsAvailable(), Is.EqualTo(false));
		}
	}
}