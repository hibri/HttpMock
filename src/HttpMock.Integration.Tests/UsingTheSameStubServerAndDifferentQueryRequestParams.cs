using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
    [TestFixture]
    public class UsingTheSameStubServerAndDifferentQueryRequestParams
    {
        //private string _endpointToHit;
        //private IHttpServer _httpMockRepository;

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
            TestContext.CurrentContext.SetCurrentHostUrl(hostUrl);
            var endpointToHit = hostUrl + "/endpoint";
            TestContext.CurrentContext.SetCurrentEndpointToHit(endpointToHit);
            TestContext.CurrentContext.SetCurrentHttpMock(HttpMockRepository.At(hostUrl));
        }

        [Test]
        public void Should_return_first_one()
        {
            var _httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            _httpMockRepository.Stub(x => x.Get("/endpoint"))
                .WithParams(_firstSetOfParams)
                .Return("I was the first one")
                .OK();

            AssertResponse("I was the first one", _firstSetOfParams);
        }

        [Test]
        public void Should_return_second_one()
        {
            var _httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            _httpMockRepository.Stub(x => x.Get("/endpoint"))
                .WithParams(_secondSetOfParams)
                .Return("I was the second one")
                .OK();

            AssertResponse("I was the second one", _secondSetOfParams);
        }

        [Test]
        public void Should_return_third_one()
        {
            var _httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            _httpMockRepository.Stub(x => x.Get("/endpoint"))
                .WithParams(_thirdSetOfParams)
                .Return("I was the third one")
                .OK();

            AssertResponse("I was the third one", _thirdSetOfParams);
        }

        private void AssertResponse(string expected, IEnumerable<KeyValuePair<string, string>> queryString)
        {
            string aggregate = queryString.Select(x => x.Key + "=" + x.Value + "&").Aggregate((a, b) => a + b).Trim('&');
            string endpointToHit = TestContext.CurrentContext.GetCurrentEndpointToHit();
            var webRequest =
                (HttpWebRequest)WebRequest.Create(string.Format("{0}?{1}&abirdinthehand=twointhebush", endpointToHit, aggregate));
            using (var response = webRequest.GetResponse())
            {
                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    string readToEnd = sr.ReadToEnd();
                    Assert.That(readToEnd, Is.EqualTo(expected));
                }
            }
        }
    }
}