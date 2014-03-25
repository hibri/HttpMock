using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerAndDifferentUris
	{
		private IHttpServer _httpMockRepository;
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
			_httpMockRepository = HttpMockRepository.At(_hostUrl);
		}

		[Test]
		public void FirstTest()
		{
			var wc = new WebClient();
			string stubbedReponse = "Response for first test";
			var stubHttp = _httpMockRepository
				.WithNewContext();

			stubHttp
				.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponse)
				.OK();

			Assert.That(wc.UploadString(string.Format("{0}/firsttest/", _hostUrl), "x"), Is.EqualTo(stubbedReponse));
		}

		[Test]
		public void SecondTest()
		{
			var wc = new WebClient();
			string stubbedReponse = "Response for second test";
			_httpMockRepository
				.WithNewContext()
				.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponse)
				.OK();

			Assert.That(wc.UploadString(string.Format("{0}/secondtest/", _hostUrl), "x"), Is.EqualTo(stubbedReponse));
		}

		[Test]
		public void Stubs_should_be_unique_within_context()
		{
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

			Assert.That(wc.UploadString(string.Format("{0}/firsttest/", _hostUrl), "x"), Is.EqualTo(stubbedReponseOne));
			Assert.That(wc.UploadString(string.Format("{0}/secondtest/", _hostUrl), "x"), Is.EqualTo(stubbedReponseTwo));
		}
	}
}