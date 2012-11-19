using System;
using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerAndDifferentUris
	{
		private IHttpServer _httpMockRepository;

		[SetUp]
		public void SetUp() {
			_httpMockRepository = HttpMockRepository.At("http://localhost:8080");
		}

		[Test]
		public void FirstTest() {
			var wc = new WebClient();
			string stubbedReponse = "Response for first test";
			var stubHttp = _httpMockRepository
				.WithNewContext();

			stubHttp
				.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponse)
				.OK();

			Assert.That(wc.UploadString("Http://localhost:8080/firsttest/", "x"), Is.EqualTo(stubbedReponse));
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

			Assert.That(wc.UploadString("Http://localhost:8080/secondtest/", "x"), Is.EqualTo(stubbedReponse));
		}

		[Test]
		public void Stubs_should_be_unique_within_context() {
			var wc = new WebClient();
			string stubbedReponseOne = "Response for first test in context";
			string stubbedReponseTwo = "Response for second test in context";

			IHttpServer stubHttp = _httpMockRepository.WithNewContext();

			stubHttp.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponseOne)
				.OK();

			stubHttp.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponseTwo)
				.OK();

			Assert.That(wc.UploadString("Http://localhost:8080/firsttest/", "x"), Is.EqualTo(stubbedReponseOne));
			Assert.That(wc.UploadString("Http://localhost:8080/secondtest/", "x"), Is.EqualTo(stubbedReponseTwo));
		}
	}
}