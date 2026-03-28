using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerAndDifferentUris
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private IHttpServer _httpMockRepository;
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
			_httpMockRepository = HttpMockRepository.At(_hostUrl);
		}

		[Test]
		public async Task FirstTest()
		{
			string stubbedReponse = "Response for first test";
			var stubHttp = _httpMockRepository
				.WithNewContext();

			stubHttp
				.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponse)
				.OK();

			var response = await _httpClient.PostAsync($"{_hostUrl}/firsttest/",
				new StringContent("x", Encoding.UTF8));
			var result = await response.Content.ReadAsStringAsync();
			Assert.That(result, Is.EqualTo(stubbedReponse));
		}

		[Test]
		public async Task SecondTest()
		{
			string stubbedReponse = "Response for second test";
			_httpMockRepository
				.WithNewContext()
				.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponse)
				.OK();

			var response = await _httpClient.PostAsync($"{_hostUrl}/secondtest/",
				new StringContent("x", Encoding.UTF8));
			var result = await response.Content.ReadAsStringAsync();
			Assert.That(result, Is.EqualTo(stubbedReponse));
		}

		[Test]
		public async Task Stubs_should_be_unique_within_context()
		{
			string stubbedReponseOne = "Response for first test in context";
			string stubbedReponseTwo = "Response for second test in context";

			IHttpServer stubHttp = _httpMockRepository.WithNewContext();

			stubHttp.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponseOne)
				.OK();

			stubHttp.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponseTwo)
				.OK();

			var response1 = await _httpClient.PostAsync($"{_hostUrl}/firsttest/",
				new StringContent("x", Encoding.UTF8));
			var result1 = await response1.Content.ReadAsStringAsync();

			var response2 = await _httpClient.PostAsync($"{_hostUrl}/secondtest/",
				new StringContent("x", Encoding.UTF8));
			var result2 = await response2.Content.ReadAsStringAsync();

			Assert.That(result1, Is.EqualTo(stubbedReponseOne));
			Assert.That(result2, Is.EqualTo(stubbedReponseTwo));
		}
	}
}