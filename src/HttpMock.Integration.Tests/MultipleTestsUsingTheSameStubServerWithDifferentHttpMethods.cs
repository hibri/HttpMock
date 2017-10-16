using System.IO;
using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
    [TestFixture]
    public class MultipleTestsUsingTheSameStubServerWithDifferentHttpMethods
    {
        [SetUp]
        public void SetUp()
        {
            var url = HostHelper.GenerateAHostUrlForAStubServerWith("endpoint");
            TestContext.CurrentContext.SetCurrentHostUrl(url);
            TestContext.CurrentContext.SetCurrentHttpMock(HttpMockRepository.At(url));
        }

        [Test]
        public void Should_get()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            httpMockRepository
                .Stub(x => x.Get("/endpoint"))
                .Return("I am a GET")
                .OK();

            AssertResponse("GET", "I am a GET");
        }

        [Test]
        public void Should_post()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            httpMockRepository
                .Stub(x => x.Post("/endpoint"))
                .Return("I am a POST")
                .OK();

            AssertResponse("POST", "I am a POST");
        }

        [Test]
        public void Should_put()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            httpMockRepository
                .Stub(x => x.Put("/endpoint"))
                .Return("I am a PUT")
                .OK();

            AssertResponse("PUT", "I am a PUT");
        }

        [Test]
        public void Should_delete()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            httpMockRepository
                .Stub(x => x.Delete("/endpoint"))
                .Return("I am a DELETE")
                .OK();

            AssertResponse("DELETE", "I am a DELETE");
        }

        [Test]
        public void Should_use_custom_verbs()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            httpMockRepository.Stub(x => x.CustomVerb("/endpoint", "PURGE")).Return("I am a PURGE").OK();
            AssertResponse("PURGE", "I am a PURGE");
        }

        [Test]
        public void Should_head()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            httpMockRepository
                .Stub(x => x.Head("/endpoint"))
                .Return("I am a HEAD")
                .OK();

            var _endpointToHit = TestContext.CurrentContext.GetCurrentHostUrl();
            var webRequest = (HttpWebRequest)WebRequest.Create(_endpointToHit);
            webRequest.Method = "HEAD";
            using (var response = webRequest.GetResponse())
            {
                Assert.That(response.Headers.Count, Is.GreaterThan(0));

            }
        }

        [Test]
        public void If_no_Mocked_Endpoints_matched_then_should_return_404_with_HttpMockError_status()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            var _endpointToHit = TestContext.CurrentContext.GetCurrentHostUrl();
            var webRequest = (HttpWebRequest)WebRequest.Create(_endpointToHit + "wibbles");
            try
            {
                using (webRequest.GetResponse())
                {
                }
            }
            catch (WebException ex)
            {
                Assert.That(((HttpWebResponse)ex.Response).StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
                Assert.That(((HttpWebResponse)ex.Response).Headers["X-HttpMockError"], Is.Not.Null, "Header not set");
            }
        }

        [Test]
        public void Should_return_dynamic_data()
        {
            var httpMockRepository = TestContext.CurrentContext.GetCurrentHttpMock();
            string value = "test1";
            httpMockRepository
                .Stub(x => x.Get("/endpoint"))
                .Return(() => value)
                .OK();
            AssertResponse("GET", "test1");
            value = "test2";
            AssertResponse("GET", "test2");
        }

        private void AssertResponse(string method, string expected)
        {
            var _endpointToHit = TestContext.CurrentContext.GetCurrentHostUrl();

            var webRequest = (HttpWebRequest)WebRequest.Create(_endpointToHit);
            webRequest.Method = method;
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