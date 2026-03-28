using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerWithDifferentHttpMethods
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private string _endpointToHit;
		private IHttpServer _httpMockRepository;

		[SetUp]
		public void SetUp()
		{
			_endpointToHit = HostHelper.GenerateAHostUrlForAStubServerWith("endpoint");
			_httpMockRepository = HttpMockRepository.At(_endpointToHit);
		}

		[Test]
		public async Task Should_get()
		{
			_httpMockRepository
				.Stub(x => x.Get("/endpoint"))
				.Return("I am a GET")
				.OK();

			await AssertResponse("GET", "I am a GET");
		}

		[Test]
		public async Task Should_post()
		{
			_httpMockRepository
				.Stub(x => x.Post("/endpoint"))
				.Return("I am a POST")
				.OK();

			await AssertResponse("POST", "I am a POST");
		}

		[Test]
		public async Task Should_put()
		{
			_httpMockRepository
				.Stub(x => x.Put("/endpoint"))
				.Return("I am a PUT")
				.OK();

			await AssertResponse("PUT", "I am a PUT");
		}

		[Test]
		public async Task Should_delete()
		{
			_httpMockRepository
				.Stub(x => x.Delete("/endpoint"))
				.Return("I am a DELETE")
				.OK();

			await AssertResponse("DELETE", "I am a DELETE");
		}

		[Test]
		public async Task Should_use_custom_verbs()
		{
			_httpMockRepository.Stub(x => x.CustomVerb("/endpoint", "PURGE")).Return("I am a PURGE").OK();
			await AssertResponse("PURGE", "I am a PURGE");
		}

		[Test]
		public async Task Should_head()
		{
			_httpMockRepository
				.Stub(x => x.Head("/endpoint"))
				.Return("I am a HEAD")
				.OK();

			var request = new HttpRequestMessage(HttpMethod.Head, _endpointToHit);
			var response = await _httpClient.SendAsync(request);
			Assert.That(response.Headers.Count(), Is.GreaterThan(0));
		}

		[Test]
		public async Task If_no_Mocked_Endpoints_matched_then_should_return_404_with_HttpMockError_status()
		{
			var response = await _httpClient.GetAsync(_endpointToHit + "wibbles");

			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
			Assert.That(response.Headers.Contains("X-HttpMockError"), Is.True, "Header not set");
		}

		[Test]
		public async Task Should_return_dynamic_data()
		{
			string value = "test1";
			_httpMockRepository
				.Stub(x => x.Get("/endpoint"))
				.Return(() => value)
				.OK();
			await AssertResponse("GET", "test1");
			value = "test2";
			await AssertResponse("GET", "test2");
		}

		private async Task AssertResponse(string method, string expected)
		{
			var request = new HttpRequestMessage(new HttpMethod(method), _endpointToHit);
			if (method == "POST" || method == "PUT" || method == "DELETE" || method == "PURGE")
				request.Content = new StringContent("", Encoding.UTF8);
			var response = await _httpClient.SendAsync(request);
			var readToEnd = await response.Content.ReadAsStringAsync();
			Assert.That(readToEnd, Is.EqualTo(expected));
		}
	}
}