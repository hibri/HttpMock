using System;
using System.Net;
using HttpMock;
using NUnit.Framework;

namespace StubHttp
{
	[TestFixture]
	public class Class2
	{
		[Test]
		public void Foo() {
			IHttpEndpoint httpEndpoint = new HttpEndpoint()
				.At(new Uri("Http://localhost:8080/api"))
				.WithNewContext();

			httpEndpoint
				.Stub(x => x.Get("/"))
				.Return("Index")
				.OK();

			httpEndpoint
				.Stub(x => x.Get("/status"))
				.Return("Hello")
				.OK();

			httpEndpoint
				.Stub(x => x.Get("/echo"))
				.Return("Echo")
				.OK();


			WebClient wc = new WebClient();
			Console.WriteLine(wc.DownloadString("Http://localhost:8080/api/"));
			Console.WriteLine(wc.DownloadString("Http://localhost:8080/api/status"));
			Console.WriteLine(wc.DownloadString("Http://localhost:8080/api/echo"));


		}
	}
}
