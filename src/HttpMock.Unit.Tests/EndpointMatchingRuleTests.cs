using System.Collections.Generic;
using Kayak.Http;
using Moq;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
    [TestFixture]
    public class EndpointMatchingRuleTests
    {
        private Mock<IRequestHandler> _mockRequestHandler;

        [SetUp]
        public void SetUp()
        {
            _mockRequestHandler = new Mock<IRequestHandler>();
        }


        [Test]
        public void urls_match_it_returns_true()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test" });
            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void urls_and_methods_the_same_it_returns_true()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("PUT");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test", Method = "PUT" });
            var endpointMatchingRule = new EndpointMatchingRule();

            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void urls_and_methods_differ_it_returns_false()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test", Method = "PUT" });
            var endpointMatchingRule = new EndpointMatchingRule();

            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead), Is.False);
        }

        [Test]
        public void urls_differ_and_methods_match_it_returns_false()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("pest");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();

            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead), Is.False);
        }

        [Test]
        public void urls_and_methods_match_queryparams_differ_it_returns_false()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams)
                .Returns(new Dictionary<string, string> { { "myParam", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();

            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead), Is.False);
        }

        [Test]
        public void urls_and_methods_match_and_queryparams_exist_it_returns_true()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams)
                .Returns(new Dictionary<string, string> { { "myParam", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead
                { Uri = "test?oauth_consumer_key=test-api&elvis=alive&moonlandings=faked&myParam=one", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();

            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void urls_and_methods_match_and_queryparams_does_not_exist_it_returns_false()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams)
                .Returns(new Dictionary<string, string> { { "myParam", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead
                { Uri = "test?oauth_consumer_key=test-api&elvis=alive&moonlandings=faked", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead), Is.False);
        }


        [Test]
        public void urls_and_methods_match_and_no_query_params_are_set_but_request_has_query_params_returns_true()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead
                { Uri = "test?oauth_consumer_key=test-api&elvis=alive&moonlandings=faked", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();

            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead), Is.True);
        }

        [Test]
        public void urls_and_methods_and_queryparams_match_it_returns_true()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams)
                .Returns(new Dictionary<string, string> { { "myParam", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test?myParam=one", Method = "GET" });

            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void urls_and_methods_match_headers_differ_it_returns_false()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());
            _mockRequestHandler.Setup(m => m.RequestHeaders)
                .Returns(new Dictionary<string, string> { { "myHeader", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead
            {
                Uri = "test",
                Method = "GET",
                Headers = new Dictionary<string, string>
                {
                    { "myHeader", "two" }
                }
            });
            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead), Is.False);
        }

        [Test]
        public void urls_and_methods_match_and_headers_match_it_returns_true()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());
            _mockRequestHandler.Setup(m => m.RequestHeaders)
                .Returns(new Dictionary<string, string> { { "myHeader", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead
            {
                Uri = "test",
                Method = "GET",
                Headers = new Dictionary<string, string>
                {
                    { "myHeader", "one" },
                    { "anotherHeader", "two" }
                }
            });
            var endpointMatchingRule = new EndpointMatchingRule();

            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void urls_and_methods_match_and_header_does_not_exist_it_returns_false()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());
            _mockRequestHandler.Setup(m => m.RequestHeaders)
                .Returns(new Dictionary<string, string> { { "myHeader", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead), Is.False);
        }

        [Test]
        public void should_do_a_case_insensitive_match_on_query_string_parameter_values()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams)
                .Returns(new Dictionary<string, string> { { "myParam", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test?myParam=OnE", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void should_do_a_case_insensitive_match_on_header_names_and_values()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());
            _mockRequestHandler.Setup(m => m.RequestHeaders)
                .Returns(new Dictionary<string, string> { { "myHeader", "one" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead
            {
                Uri = "test",
                Method = "GET",
                Headers = new Dictionary<string, string>
                {
                    { "MYheaDER", "OnE" }
                }
            });

            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void should_match_when_the_query_string_has_a_trailing_ampersand()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("test");
            _mockRequestHandler.Setup(m => m.Method).Returns("GET");
            _mockRequestHandler.Setup(m => m.QueryParams)
                .Returns(new Dictionary<string, string> { { "a", "b" }, { "c", "d" } });

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "test?a=b&c=d&", Method = "GET" });
            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }

        [Test]
        public void should_match_urls_containings_regex_reserved_characters()
        {
            _mockRequestHandler.Setup(m => m.Path).Returns("/test()");
            _mockRequestHandler.Setup(m => m.QueryParams).Returns(new Dictionary<string, string>());

            var httpRequestHead = new KayakHttpRequestHeadAdapter(new HttpRequestHead { Uri = "/test()" });
            var endpointMatchingRule = new EndpointMatchingRule();
            Assert.That(endpointMatchingRule.IsEndpointMatch(_mockRequestHandler.Object, httpRequestHead));
        }
    }
}