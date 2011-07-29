using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using HttpMock;
using Kayak;
using Kayak.Http;
using NUnit.Framework;
using Rhino.Mocks;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpEndPointTests
	{
		[Test]
		public void SUT_should_return_stubbed_response() {
			IStubHttp stubHttp = HttpMockRepository
				.At("http://localhost:8080/someapp");


			const string expected = "<xml><>response>Hello World</response></xml>";
			stubHttp.Stub(x => x.Get("/someendpoint"))
				.Return(expected)
				.OK();

			string result = new SystemUnderTest().GetData();

			Assert.That(result, Is.EqualTo(expected));
			
		}

		//[Test]
		//public void Spike() {
		//    IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 10001);
		//    var schedulerDelegate = MockRepository.GenerateStub<ISchedulerDelegate>(); ;
		//    //scheduler.Post(() => Begin(ipEndPoint));
		//    Begin(ipEndPoint);
		//    Task.Factory.StartNew(() => Begin(ipEndPoint));
		//}

		//private static void Begin(IPEndPoint ipEndPoint) {
		//    var httpRequestDelegate = MockRepository.GenerateStub<IHttpRequestDelegate>();
		//    KayakServer.Factory
		//                .CreateHttp(httpRequestDelegate)
		//                .Listen(ipEndPoint);
		//}

		[Test]
		public void Should_return_expected_ok_response() {
			IStubHttp stubHttp = HttpMockRepository.At(new Uri("Http://localhost:8080/api"));

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

			
		}
	}
}