using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingTheSameStubServerAndDifferentQueryRequestParams
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private string _endpointToHit;
		private IHttpServer _httpMockRepository;

		private readonly IDictionary<string, string> _firstSetOfParams = new Dictionary<string, string>
		{
			{"trackId", "1"},
			{"formatId", "1"}
		};

		private readonly IDictionary<string, string> _secondSetOfParams = new Dictionary<string, string>
		{
			{"trackId", "2"},
			{"formatId", "2"}
		};

		private readonly IDictionary<string, string> _thirdSetOfParams = new Dictionary<string, string>
		{
			{"trackId", "3"},
			{"formatId", "3"}
		};

		[SetUp]
		public void SetUp()
		{
			var hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
			_endpointToHit = hostUrl + "/endpoint";
			_httpMockRepository = HttpMockRepository.At(hostUrl);
		}

		[Test]
		public async Task Should_return_first_one()
		{
			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithParams(_firstSetOfParams)
				.Return("I was the first one")
				.OK();

			await AssertResponse("I was the first one", _firstSetOfParams);
		}

		[Test]
		public async Task Should_return_second_one()
		{
			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithParams(_secondSetOfParams)
				.Return("I was the second one")
				.OK();

			await AssertResponse("I was the second one", _secondSetOfParams);
		}

		[Test]
		public async Task Should_return_third_one()
		{
			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithParams(_thirdSetOfParams)
				.Return("I was the third one")
				.OK();

			await AssertResponse("I was the third one", _thirdSetOfParams);
		}

		private async Task AssertResponse(string expected, IEnumerable<KeyValuePair<string, string>> queryString)
		{
			string aggregate = queryString.Select(x => x.Key + "=" + x.Value + "&").Aggregate((a, b) => a + b).Trim('&');

			var readToEnd = await _httpClient.GetStringAsync(
				$"{_endpointToHit}?{aggregate}&abirdinthehand=twointhebush");
			Assert.That(readToEnd, Is.EqualTo(expected));
		}
	}
}