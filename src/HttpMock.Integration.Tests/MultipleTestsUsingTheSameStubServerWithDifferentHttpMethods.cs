using System.IO;
using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerWithDifferentHttpMethods
	{
		private const string ENDPOINT_TO_HIT = "http://localhost:9191/endpoint";
		private IStubHttp _httpMockRepository;

		[TestFixtureSetUp]
		public void SetUp() {
			_httpMockRepository = HttpMockRepository.At("http://localhost:9191/");

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
				Assert.That(response.Headers.Count, Is.GreaterThan(0));
				Assert.That(response.GetResponseStream().CanSeek, Is.False);
			}
		}

		[Test]
		public void Should_head_fail() {
			var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost:9191/endpoint?param=one");
			webRequest.Method = "HEAD";
			try {
				using (var response = webRequest.GetResponse()) {
					Assert.That(response.Headers.Count, Is.GreaterThan(0));
					Assert.That(response.GetResponseStream().CanSeek, Is.False);
				}
			} catch(WebException ex){
				Assert.That(((HttpWebResponse)ex.Response).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
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
}