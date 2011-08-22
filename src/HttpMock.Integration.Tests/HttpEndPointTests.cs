using System;
using System.Net;
using System.Net.Sockets;
using HttpMock;
using NUnit.Framework;
using log4net.Config;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpEndPointTests
	{
		private IHttpServer _stubHttp;

		[Test]
		public void SUT_should_return_stubbed_response() {
			_stubHttp = HttpMockRepository.At("http://localhost:9191/");

			const string expected = "<xml><>response>Hello World</response></xml>";
			_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

			Console.WriteLine(_stubHttp.WhatDoIHave());

			string result = 	new WebClient().DownloadString("http://localhost:9191/endpoint");

			Console.WriteLine("RESPONSE: {0}", result);
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void Should_start_listening_before_stubs_have_been_set() {
			_stubHttp = HttpMockRepository.At("http://localhost:9191/");

			_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return("listening")
				.OK();

			using (var tcpClient = new TcpClient()) {
				tcpClient.Connect(new Uri("Http://localhost:9191/").Host, new Uri("Http://localhost:9191/").Port);

				Assert.That(tcpClient.Connected, Is.True);

				tcpClient.Close();
			}

			string result = new WebClient().DownloadString("http://localhost:9191/endpoint");

			Console.WriteLine("RESPONSE: {0}", result);
			Assert.That(result, Is.EqualTo("listening"));
		}

		[Test]
		public void Should_return_expected_ok_response() {
			string endpoint = "Http://localhost:9191/";
			_stubHttp = HttpMockRepository.At(endpoint);

			_stubHttp
				.Stub(x => x.Get("/api2/status"))
				.Return("Hello")
				.OK();

			_stubHttp
				.Stub(x => x.Get("/api2/echo"))
				.Return("Echo")
				.NotFound();

			_stubHttp
				.Stub(x => x.Get("/api2/echo2"))
				.Return("Nothing")
				.WithStatus(HttpStatusCode.Unauthorized);

			Console.WriteLine(_stubHttp.WhatDoIHave());

			var wc = new WebClient();

			Assert.That(wc.DownloadString(string.Format("{0}api2/status", endpoint)), Is.EqualTo("Hello"));

			try {
				Console.WriteLine(wc.DownloadString(endpoint + "api2/echo"));
			}
			catch (Exception ex) {
				Assert.That(ex, Is.InstanceOf(typeof (WebException)));
				Assert.That(((WebException) ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}

			try
			{
				Console.WriteLine(wc.DownloadString(endpoint + "api2/echo2"));
			}
			catch (Exception ex)
			{
				Assert.That(ex, Is.InstanceOf(typeof(WebException)));
				Assert.That(((WebException)ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}
		}


		[Test]
		public void Should_hit_the_same_url_multiple_times()
		{
			string endpoint = "Http://localhost:9191/";
			_stubHttp = HttpMockRepository.At(endpoint);

			

			_stubHttp
				.Stub(x => x.Get("/api2/echo"))
				.Return("Echo")
				.NotFound();

			_stubHttp
				.Stub(x => x.Get("/api2/echo2"))
				.Return("Nothing")
				.WithStatus(HttpStatusCode.Unauthorized);

			Console.WriteLine(_stubHttp.WhatDoIHave());

			for (int count = 0; count < 6; count++) {
				RequestEcho(endpoint);	
			}
			
		}


		private static void RequestEcho(string endpoint) {
			var wc = new WebClient();

			try {
				Console.WriteLine(wc.DownloadString(endpoint + "api2/echo"));
			} catch (Exception ex) {
				Assert.That(ex, Is.InstanceOf(typeof (WebException)));
				Assert.That(((WebException) ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}
		}

	}

	[SetUpFixture]
	public class AssemblySetup
	{
		public AssemblySetup() {
			XmlConfigurator.Configure();
		}
	}

}