using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	public class StubConstraintsTests
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private IHttpServer _httpMockRepository;
		private IHttpServer _stubHttp;
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
			_httpMockRepository = HttpMockRepository.At(_hostUrl);
			_stubHttp = _httpMockRepository.WithNewContext();
		}

		[Test]
		public async Task Constraints_can_be_applied_to_urls()
		{
			_stubHttp
				.Stub(x => x.Post("/firsttest"))
				.WithUrlConstraint(url => url.Contains("/blah/blah") == false)
				.Return("<Xml>ShouldntBeReturned</Xml>")
				.OK();

			var response = await _httpClient.PostAsync(
				$"{_hostUrl}/firsttest/blah/blah",
				new StringContent("x", Encoding.UTF8));

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
		}
	}
}