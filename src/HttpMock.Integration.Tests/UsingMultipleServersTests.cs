using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingMultipleServersTests
	{
		[Test]
		public void Using8080() {
			string expected = "expected response";
			HttpMockRepository.At("http://localhost:8080/app")
				.Stub( x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();
			Assert.That(wc.DownloadString("http://localhost:8080/app/endpoint"), Is.EqualTo(expected));
		}


		[Test]
		public void Using8081()
		{
			string expected = "expected response";
			HttpMockRepository.At("http://localhost:8081/app")
				.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();
			Assert.That(wc.DownloadString("http://localhost:8081/app/endpoint"), Is.EqualTo(expected));
		}

		[Test]
		public void Using9081()
		{
			string expected = "expected response";
			HttpMockRepository.At("http://localhost:9081/app")
				.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();
			Assert.That(wc.DownloadString("http://localhost:9081/app/endpoint"), Is.EqualTo(expected));
		}
	}
}