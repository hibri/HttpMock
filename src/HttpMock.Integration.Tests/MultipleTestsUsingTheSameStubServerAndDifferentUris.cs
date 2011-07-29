using System;
using System.IO;
using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerWithDifferentHttpMethods
	{
		private const string ENDPOINT_TO_HIT = "http://localhost:8080/endpoint";
		private IStubHttp _httpMockRepository;

		[TestFixtureSetUp]
		public void SetUp() {
			_httpMockRepository = HttpMockRepository.At("http://localhost:8080/");

			_httpMockRepository
				.Stub(x => x.Get("/endpoint"))
				.Return("I am a GET")
				.OK();

			_httpMockRepository
				.Stub(x => x.Post("/endpoint"))
				.Return("I am a POST")
				.OK();

			_httpMockRepository
				.Stub(x => x.Put("/endpoint"))
				.Return("I am a PUT")
				.OK();

			_httpMockRepository
				.Stub(x => x.Delete("/endpoint"))
				.Return("I am a DELETE")
				.OK();

			_httpMockRepository
				.Stub(x => x.Head("/endpoint"))
				.Return("I am a HEAD")
				.OK();

		}

		[Test]
		public void Should_get() {
			AssertResponse("GET", "I am a GET");
		}

		[Test]
		public void Should_post() {
			AssertResponse("POST", "I am a POST");
		}

		[Test]
		public void Should_put() {
			AssertResponse("PUT", "I am a PUT");
		}

		[Test]
		public void Should_delete() {
			AssertResponse("DELETE", "I am a DELETE");
		}

		[Test]
		public void Should_head() {
			var webRequest = (HttpWebRequest)WebRequest.Create(ENDPOINT_TO_HIT);
			webRequest.Method = "HEAD";
			using (var response = webRequest.GetResponse()) { 
				
			}
		}

		private void AssertResponse(string method, string expected) {

			var webRequest = (HttpWebRequest)WebRequest.Create(ENDPOINT_TO_HIT);
			webRequest.Method = method;
			using(var response = webRequest.GetResponse()) {
				using(var sr = new StreamReader(response.GetResponseStream()))
				{
					string readToEnd = sr.ReadToEnd();
					Assert.That(readToEnd, Is.EqualTo(expected));
				}
			}
		}
	}


	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerAndDifferentUris
	{
		private IStubHttp _httpMockRepository;

		[TestFixtureSetUp]
		public void SetUp() {
			_httpMockRepository = HttpMockRepository.At("http://localhost:8080/");
		}

		[Test]
		public void FirstTest() {
			var wc = new WebClient();
			string stubbedReponse = "Response for first test";
			_httpMockRepository
				.WithNewContext()
				.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponse)
				.OK();

			Assert.That(wc.UploadString("Http://localhost:8080/firsttest/", ""), Is.EqualTo(stubbedReponse));
		}

		[Test]
		public void SecondTest() {
			var wc = new WebClient();
			string stubbedReponse = "Response for second test";
			_httpMockRepository
				.WithNewContext()
				.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponse)
				.OK();

			Assert.That(wc.UploadString("Http://localhost:8080/secondtest/", ""), Is.EqualTo(stubbedReponse));
		}

		[Test]
		public void Stubs_should_be_unique_within_context() {
			var wc = new WebClient();
			string stubbedReponseOne = "Response for first test in context";
			string stubbedReponseTwo = "Response for second test in context";

			IStubHttp stubHttp = _httpMockRepository.WithNewContext();

			stubHttp.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponseOne)
				.OK();

			stubHttp.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponseTwo)
				.OK();

			Assert.That(wc.UploadString("Http://localhost:8080/firsttest/", ""), Is.EqualTo(stubbedReponseOne));
			Assert.That(wc.UploadString("Http://localhost:8080/secondtest/", ""), Is.EqualTo(stubbedReponseTwo));
		}
	}
}