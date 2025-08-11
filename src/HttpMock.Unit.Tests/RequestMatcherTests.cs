using System.Collections.Generic;
using Kayak.Http;
using Moq;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
    [TestFixture]
    public class RequestMatcherTests
    {
        [Test]
        public void Should_match_a_handler()
        {
            var expectedRequest = new Mock<IRequestHandler>();
            expectedRequest.SetupGet(x => x.Method).Returns("GET");
            expectedRequest.SetupGet(x => x.Path).Returns("/path");
            expectedRequest.SetupGet(x => x.QueryParams).Returns(new Dictionary<string, string>());
            expectedRequest.Setup(x => x.CanVerifyConstraintsFor(It.IsAny<string>())).Returns(true);

            var requestMatcher = new RequestMatcher(new EndpointMatchingRule());
            var requestHandlerList = new List<IRequestHandler> { expectedRequest.Object };


            var httpRequestHead = new HttpRequestHead { Method = "GET", Path = "/path/", Uri = "/path" };

            var matchedRequest = requestMatcher.Match(httpRequestHead, requestHandlerList);


            Assert.That(matchedRequest.Path, Is.EqualTo("/path"));
        }


        [Test]
        public void Should_match_a_specific_handler()
        {
            var expectedRequest = new Mock<IRequestHandler>();
            expectedRequest.SetupGet(x => x.Method).Returns("GET");
            expectedRequest.SetupGet(x => x.Path).Returns("/path/specific");
            expectedRequest.SetupGet(x => x.QueryParams).Returns(new Dictionary<string, string>());
            expectedRequest.Setup(x => x.CanVerifyConstraintsFor(It.IsAny<string>())).Returns(true);

            var otherRequest = new Mock<IRequestHandler>();
            otherRequest.SetupGet(x => x.Method).Returns("GET");
            otherRequest.SetupGet(x => x.Path).Returns("/path/");
            otherRequest.SetupGet(x => x.QueryParams).Returns(new Dictionary<string, string>());
            otherRequest.Setup(x => x.CanVerifyConstraintsFor(It.IsAny<string>())).Returns(true);

            var requestMatcher = new RequestMatcher(new EndpointMatchingRule());
            var requestHandlerList = new List<IRequestHandler> { otherRequest.Object, expectedRequest.Object };


            var httpRequestHead = new HttpRequestHead { Method = "GET", Path = "/path/specific", Uri = "/path/specific" };

            var matchedRequest = requestMatcher.Match(httpRequestHead, requestHandlerList);


            Assert.That(matchedRequest.Path, Is.EqualTo("/path/specific"));
        }
    }
}