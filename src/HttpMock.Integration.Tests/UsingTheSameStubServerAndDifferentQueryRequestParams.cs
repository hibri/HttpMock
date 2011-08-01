using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingTheSameStubServerAndDifferentQueryRequestParams
	{
		private const string ENDPOINT_TO_HIT = "http://localhost:9191/endpoint";
		private IStubHttp _httpMockRepository;
		private readonly IDictionary<string,string> _firstSetOfParams = new Dictionary<string, string>{{"firstArg","1"}};
		private readonly IDictionary<string, string> _secondSetOfParams = new Dictionary<string, string> { { "secondArg", "2" } };
		private readonly IDictionary<string, string> _thirdSetOfParams = new Dictionary<string, string> { { "thirdArg", "3" } };

		[TestFixtureSetUp]
		public void SetUp() {
			_httpMockRepository = HttpMockRepository.At("http://localhost:9191/");

			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithParams(_firstSetOfParams)
				.Return("I was the first one")
				.OK();

			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithParams(_secondSetOfParams)
				.Return("I was the second one")
				.OK();

			_httpMockRepository.Stub(x => x.Get("/endpoint"))
				.WithParams(_thirdSetOfParams)
				.Return("I was the third one")
				.OK();
		}

		[Test]
		public void Should_return_first_one() {
			AssertResponse("I was the first one", _firstSetOfParams);
		}

		[Test]
		public void Should_return_second_one() {
			AssertResponse("I was the second one", _secondSetOfParams);
		}

		[Test]
		public void Should_return_third_one() {
			AssertResponse("I was the third one", _thirdSetOfParams);
		}

		private void AssertResponse(string expected, IEnumerable<KeyValuePair<string, string>> queryString) {
			
			string aggregate = queryString.Select(x => x.Key + "=" + x.Value + "&").Aggregate((a, b) => a + b).Trim('&');

			var webRequest = (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}", ENDPOINT_TO_HIT, aggregate));
			using (var response = webRequest.GetResponse()) {
				using (var sr = new StreamReader(response.GetResponseStream())) {
					string readToEnd = sr.ReadToEnd();
					Assert.That(readToEnd, Is.EqualTo(expected));
				}
			}
		}

	}
}