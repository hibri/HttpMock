using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingTheSameStubServerAndDifferentRequestHeaders
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private string _endpointToHit;
		private IHttpServer _httpMockRepository;

		private readonly IDictionary<string, string> _firstSetOfHeaders = new Dictionary<string, string>
		{
			{"X-HeaderOne", "one"},
			{"X-HeaderTwo", "a"}
		};

		private readonly IDictionary<string, string> _secondSetOfHeaders = new Dictionary<string, string>
		{
			{"X-HeaderOne", "one"},
			{"X-HeaderTwo", "b"}
		};

		private readonly IDictionary<string, string> _thirdSetOfHeaders = new Dictionary<string, string>
		{
			{"X-HeaderOne", "two"},
			{"X-HeaderTwo", "a"}
		};

		[SetUp]
		public void SetUp()
		{
			var hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
			_endpointToHit = hostUrl + "/endpoint";
			_httpMockRepository = HttpMockRepository.At(hostUrl);

			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithHeaders(_firstSetOfHeaders)
				.Return("I was the first one")
				.OK();
			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithHeaders(_secondSetOfHeaders)
				.Return("I was the second one")
				.OK();
			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithHeaders(_thirdSetOfHeaders)
				.Return("I was the third one")
				.OK();
		}

		[Test]
		public async Task Should_return_first_one()
		{
			await AssertResponse("I was the first one", _firstSetOfHeaders);
		}

		[Test]
		public async Task Should_return_second_one()
		{
			await AssertResponse("I was the second one", _secondSetOfHeaders);
		}

		[Test]
		public async Task Should_return_third_one()
		{
			await AssertResponse("I was the third one", _thirdSetOfHeaders);
		}

		private async Task AssertResponse(string expected, IEnumerable<KeyValuePair<string, string>> headers)
		{
			var request = new HttpRequestMessage(HttpMethod.Get,
				$"{_endpointToHit}?abirdinthehand=twointhebush");
			foreach (var header in headers)
			{
				request.Headers.Add(header.Key, header.Value);
			}
			var response = await _httpClient.SendAsync(request);
			var readToEnd = await response.Content.ReadAsStringAsync();
			Assert.That(readToEnd, Is.EqualTo(expected));
		}
	}
}