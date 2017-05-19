using System.Collections.Generic;
using Kayak.Http;
using NUnit.Framework;
using Rhino.Mocks;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class EndpointMatchingRuleTests
	{
		[Test]
		public void urls_match_it_returns_true( ) {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.QueryParams = new Dictionary<string, string>();

			var httpRequestHead = new HttpRequestHead { Uri = "test" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
		}

		[Test]
		public void urls_and_methods_the_same_it_returns_true() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "PUT";
			requestHandler.QueryParams = new Dictionary<string, string>();

			var httpRequestHead = new HttpRequestHead { Uri = "test", Method = "PUT" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
		}

		[Test]
		public void urls_and_methods_differ_it_returns_false() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string>();
			var httpRequestHead = new HttpRequestHead { Uri = "test", Method = "PUT" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead), Is.False);
		}

		[Test]
		public void urls_differ_and_methods_match_it_returns_false() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "pest";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string>();
			var httpRequestHead = new HttpRequestHead { Uri = "test", Method = "GET" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead), Is.False);
		}

		[Test]
		public void urls_and_methods_match_queryparams_differ_it_returns_false() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string> { { "myParam", "one" } };

			var httpRequestHead = new HttpRequestHead { Uri = "test", Method = "GET" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead), Is.False);
		}

		[Test]
		public void urls_and_methods_match_and_queryparams_exist_it_returns_true() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string> { { "myParam", "one" } };

			var httpRequestHead = new HttpRequestHead { Uri = "test?oauth_consumer_key=test-api&elvis=alive&moonlandings=faked&myParam=one", Method = "GET" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
		}

		[Test]
		public void urls_and_methods_match_and_queryparams_does_not_exist_it_returns_false() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string> { { "myParam", "one" } };

			var httpRequestHead = new HttpRequestHead { Uri = "test?oauth_consumer_key=test-api&elvis=alive&moonlandings=faked", Method = "GET" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead), Is.False);
		}


		[Test]
		public void urls_and_methods_match_and_no_query_params_are_set_but_request_has_query_params_returns_true()
		{
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string> ();

			var httpRequestHead = new HttpRequestHead { Uri = "test?oauth_consumer_key=test-api&elvis=alive&moonlandings=faked", Method = "GET" };
			var endpointMatchingRule = new EndpointMatchingRule();

			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead), Is.True);
		}

		[Test]
		public void urls_and_methods_and_queryparams_match_it_returns_true() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string>{{"myParam", "one"}};

			var httpRequestHead = new HttpRequestHead { Uri = "test?myParam=one", Method = "GET" };
			
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
		}

		[Test]
		public void urls_and_methods_match_headers_differ_it_returns_false() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string>();
			requestHandler.RequestHeaders = new Dictionary<string, string> { { "myHeader", "one" } };

			var httpRequestHead = new HttpRequestHead
			{
				Uri = "test",
				Method = "GET",
				Headers = new Dictionary<string, string>
				{
					{ "myHeader", "two" }
				}
			};
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead), Is.False);
		}

		[Test]
		public void urls_and_methods_match_and_headers_match_it_returns_true() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string>();
			requestHandler.RequestHeaders = new Dictionary<string, string> { { "myHeader", "one" } };

			var httpRequestHead = new HttpRequestHead
			{
				Uri = "test",
				Method = "GET",
				Headers = new Dictionary<string, string>
				{
					{ "myHeader", "one" },
					{ "anotherHeader", "two" }
				}
			};
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
		}

		[Test]
		public void urls_and_methods_match_and_header_does_not_exist_it_returns_false() {
			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string>();
			requestHandler.RequestHeaders = new Dictionary<string, string> { { "myHeader", "one" } };

			var httpRequestHead = new HttpRequestHead { Uri = "test", Method = "GET" };
			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead), Is.False);
		}

		[Test]
		public void should_do_a_case_insensitive_match_on_query_string_parameter_values() {

			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string> { { "myParam", "one" } };

			var httpRequestHead = new HttpRequestHead { Uri = "test?myParam=OnE", Method = "GET" };

			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
		}

		[Test]
		public void should_do_a_case_insensitive_match_on_header_names_and_values() {

			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string>();
			requestHandler.RequestHeaders = new Dictionary<string, string> { { "myHeader", "one" } };

			var httpRequestHead = new HttpRequestHead
			{
				Uri = "test",
				Method = "GET",
				Headers = new Dictionary<string, string>
				{
					{ "MYheaDER", "OnE" }
				}
			};

			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
		}

		[Test]
		public void should_match_when_the_query_string_has_a_trailing_ampersand()
		{

			var requestHandler = MockRepository.GenerateStub<IRequestHandler>();
			requestHandler.Path = "test";
			requestHandler.Method = "GET";
			requestHandler.QueryParams = new Dictionary<string, string> { { "a", "b" } ,{"c","d"}};

			var httpRequestHead = new HttpRequestHead { Uri = "test?a=b&c=d&", Method = "GET" };

			var endpointMatchingRule = new EndpointMatchingRule();
			Assert.That(endpointMatchingRule.IsEndpointMatch(requestHandler, httpRequestHead));
			
		}


	}


    [TestFixture]
    public class RequestMatcherTests
    {
        [Test]
        public void Should_match_a_handler()
        {
            var expectedRequest = MockRepository.GenerateStub<IRequestHandler>();
            expectedRequest.Method = "GET";
            expectedRequest.Path = "/path";
            expectedRequest.QueryParams = new Dictionary<string, string>();
            expectedRequest.Stub(s => s.CanVerifyConstraintsFor("")).IgnoreArguments().Return(true);

            var requestMatcher = new RequestMatcher(new EndpointMatchingRule());

            var requestHandlerList = new List<IRequestHandler>{expectedRequest};


            var httpRequestHead = new HttpRequestHead{Method = "GET", Path = "/path/", Uri = "/path"};

            var matchedRequest = requestMatcher.Match(httpRequestHead, requestHandlerList);


            Assert.That(matchedRequest.Path, Is.EqualTo(expectedRequest.Path));
        }


        [Test]
        public void Should_match_a_specific_handler()
        {
            var expectedRequest = MockRepository.GenerateStub<IRequestHandler>();
            expectedRequest.Method = "GET";
            expectedRequest.Path = "/path/specific";
            expectedRequest.QueryParams = new Dictionary<string, string>();
            expectedRequest.Stub(s => s.CanVerifyConstraintsFor("")).IgnoreArguments().Return(true);


            var otherRequest = MockRepository.GenerateStub<IRequestHandler>();
            otherRequest.Method = "GET";
            otherRequest.Path = "/path/";
            otherRequest.QueryParams = new Dictionary<string, string>();
            otherRequest.Stub(s => s.CanVerifyConstraintsFor("")).IgnoreArguments().Return(true);

            var requestMatcher = new RequestMatcher(new EndpointMatchingRule());

            var requestHandlerList = new List<IRequestHandler> {  otherRequest, expectedRequest };


            var httpRequestHead = new HttpRequestHead { Method = "GET", Path = "/path/specific", Uri = "/path/specific" };

            var matchedRequest = requestMatcher.Match(httpRequestHead, requestHandlerList);


            Assert.That(matchedRequest.Path, Is.EqualTo(expectedRequest.Path));
        }
    }
}