using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	public class StubConstraintsTests
	{
		private IHttpServer _httpMockRepository;
		private WebClient _wc;
		private IHttpServer _stubHttp;
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
			_httpMockRepository = HttpMockRepository.At(_hostUrl);
			_wc = new WebClient();
			_stubHttp = _httpMockRepository.WithNewContext();
		}

		[Test]
		public void Constraints_can_be_applied_to_urls()
		{
			_stubHttp
				.Stub(x => x.Post("/firsttest"))
				.WithUrlConstraint(url => url.Contains("/blah/blah") == false)
				.Return("<Xml>ShouldntBeReturned</Xml>")
				.OK();

			try
			{
				_wc.UploadString(string.Format("{0}/firsttest/blah/blah", _hostUrl), "x");

				Assert.Fail("Should have 404d");
			}
			catch (WebException ex)
			{
				Assert.That(ex.Message, Is.EqualTo("The remote server returned an error: (404) Not Found."));
			}
		}
	}
}