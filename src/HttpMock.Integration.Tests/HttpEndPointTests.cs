using System;
using System.Net;
using HttpMock;
using NUnit.Framework;

namespace StubHttp
{
	[TestFixture]
	public class HttpEndPointTests
	{
		[Test]
		public void SUT_should_return_stubbed_response() {
			IHttpEndpoint httpEndpoint = new HttpEndpoint()
				.At("http://localhost:8083/someapp");

			IStubHttp stubHttp = httpEndpoint
				.WithNewContext();

			const string expected = "<xml><>response>Hello World</response></xml>";
			stubHttp.Stub(x => x.Get("/someendpoint"))
				.Return(expected)
				.OK();

			string result = new SystemUnderTest().GetData();

			Assert.That(result, Is.EqualTo(expected));
			httpEndpoint.Dispose();
		}

		

		[Test]
		public void Should_return_expected_ok_response() {
			var httpEndPoint = new HttpEndpoint()
				.At(new Uri("Http://localhost:8080/api"));
			var stubHttp = httpEndPoint
				.WithNewContext();

			stubHttp
				.Stub(x => x.Get("/"))
				.Return("Index")
				.OK();

			stubHttp
				.Stub(x => x.Get("/status"))
				.Return("Hello")
				.OK();

			stubHttp
				.Stub(x => x.Get("/echo"))
				.Return("Echo")
				.NotFound();

			stubHttp
				.Stub(x => x.Get("/echo2"))
				.Return("Nothing")
				.WithStatus(HttpStatusCode.Unauthorized);


			var wc = new WebClient();
			Console.WriteLine(wc.DownloadString("Http://localhost:8080/api/"));
			Console.WriteLine(wc.DownloadString("Http://localhost:8080/api/status"));
			try {
				Console.WriteLine(wc.DownloadString("Http://localhost:8080/api/echo"));
			}
			catch (Exception ex) {
				Assert.That(ex, Is.InstanceOf(typeof (WebException)));
				Assert.That(((WebException) ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}

			try
			{
				Console.WriteLine(wc.DownloadString("Http://localhost:8080/api/echo2"));
			}
			catch (Exception ex)
			{
				Assert.That(ex, Is.InstanceOf(typeof(WebException)));
				Assert.That(((WebException)ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}

			httpEndPoint.Dispose();
		}
	}
}