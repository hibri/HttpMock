using System.Collections.Generic;
using System.IO;
using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingTheSameStubServerAndDifferentRequestHeaders
	{
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
		public void Should_return_first_one()
		{
			AssertResponse("I was the first one", _firstSetOfHeaders);
		}

		[Test]
		public void Should_return_second_one()
		{
			AssertResponse("I was the second one", _secondSetOfHeaders);
		}

		[Test]
		public void Should_return_third_one()
		{
			AssertResponse("I was the third one", _thirdSetOfHeaders);
		}

		private void AssertResponse(string expected, IEnumerable<KeyValuePair<string, string>> headers)
		{
			var webRequest =
				(HttpWebRequest) WebRequest.Create(string.Format("{0}?abirdinthehand=twointhebush", _endpointToHit));
			foreach (var header in headers)
			{
				webRequest.Headers.Add(header.Key, header.Value);
			}
			using (var response = webRequest.GetResponse())
			{
				using (var sr = new StreamReader(response.GetResponseStream()))
				{
					var readToEnd = sr.ReadToEnd();
					Assert.That(readToEnd, Is.EqualTo(expected));
				}
			}
		}
	}
}