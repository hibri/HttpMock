using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
    [TestFixture]
    public class MultipleTestsUsingTheSameStubServerAndParallelScenarios
    {
        private string hostUrl;
        private IHttpServer stubHttp;

        [OneTimeSetUp]
        public void SetUp()
        {
            hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
            stubHttp = HttpMockRepository.At(hostUrl);
        }

        [Test, Repeat(50)]
        public void FirstTestWithSession()
        {
            var wc = new WebClient();
            string sessionId = Guid.NewGuid().ToString();
            string stubbedReponse = $"Response for first test with {sessionId}";

            stubHttp
                .Stub(x => x.Post("/firsttest"))
                .WithHeaders(new Dictionary<string, string>() { { Constants.MockSessionHeaderKey, sessionId } })
                .Return(stubbedReponse)
                .OK();
            wc.Headers.Add(Constants.MockSessionHeaderKey, sessionId);
            Assert.That(wc.UploadString(string.Format("{0}/firsttest/", hostUrl), "x"), Is.EqualTo(stubbedReponse));
        }

        [Test, Repeat(50)]
        public void SecondTestWithSession()
        {
            var wc = new WebClient();
            string sessionId = Guid.NewGuid().ToString();
            string stubbedReponse = $"Response for second test with {sessionId}";
            stubHttp
                .Stub(x => x.Post("/secondtest"))
                .WithHeaders(new Dictionary<string, string>() { { Constants.MockSessionHeaderKey, sessionId } })
                .Return(stubbedReponse)
                .OK();

            wc.Headers.Add(Constants.MockSessionHeaderKey, sessionId);
            Assert.That(wc.UploadString(string.Format("{0}/secondtest/", hostUrl), "x"), Is.EqualTo(stubbedReponse));
        }

        [Test, Repeat(50)]
        public void Stubs_should_be_unique_within_contextWithSession()
        {
            var wc = new WebClient();
            string sessionId = Guid.NewGuid().ToString();
            string stubbedReponseOne = $"Response for first test in context with {sessionId}";
            string stubbedReponseTwo = $"Response for second test in context with {sessionId}";

            stubHttp.Stub(x => x.Post("/firsttest"))
                .WithHeaders(new Dictionary<string, string>() { { Constants.MockSessionHeaderKey, sessionId } })
                .Return(stubbedReponseOne)
                .OK();

            stubHttp.Stub(x => x.Post("/secondtest"))
                .WithHeaders(new Dictionary<string, string>() { { Constants.MockSessionHeaderKey, sessionId } })
                .Return(stubbedReponseTwo)
                .OK();

            wc.Headers.Add(Constants.MockSessionHeaderKey, sessionId);
            Assert.That(wc.UploadString(string.Format("{0}/firsttest/", hostUrl), "x"), Is.EqualTo(stubbedReponseOne));
            Assert.That(wc.UploadString(string.Format("{0}/secondtest/", hostUrl), "x"), Is.EqualTo(stubbedReponseTwo));
        }

        [Test, Repeat(50)]
        public void Stubs_should_Clear_PrevStub_WithNewContext_AndContextWithSession()
        {
            var wc = new WebClient();
            Guid sessionId = Guid.NewGuid();
            string stubbedReponse = $"Response for first test in context with {sessionId}";

            stubHttp.Stub(x => x.Post("/firsttest"))
                .WithHeaders(new Dictionary<string, string>() { { Constants.MockSessionHeaderKey, sessionId.ToString() } })
                .Return(stubbedReponse)
                .OK();
            wc.Headers.Add(Constants.MockSessionHeaderKey, sessionId.ToString());
            Assert.That(wc.UploadString(string.Format("{0}/firsttest/", hostUrl), "x"), Is.EqualTo(stubbedReponse));

            wc = new WebClient();
            stubHttp.WithNewContext(sessionId);
            stubbedReponse = $"Response for edited first test in context with {sessionId}";
            stubHttp.Stub(x => x.Post("/firsttest"))
                .WithHeaders(new Dictionary<string, string>() { { Constants.MockSessionHeaderKey, sessionId.ToString() } })
                .Return(stubbedReponse)
                .OK();

            wc.Headers.Add(Constants.MockSessionHeaderKey, sessionId.ToString());
            Assert.That(wc.UploadString(string.Format("{0}/firsttest/", hostUrl), "x"), Is.EqualTo(stubbedReponse));
        }



    }
}