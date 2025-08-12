using System.Collections.Generic;
using System.Linq;
using Kayak;
using Kayak.Http;
using Moq;
using NUnit.Framework;

namespace HttpMock.Unit.Tests {
	[TestFixture]
	public class RequestProcessorTests
	{
		private RequestProcessor _processor;
		private Mock<IDataProducer> _dataProducer;
		private Mock<IHttpResponseDelegate> _httpResponseDelegate;
		private Mock<IMatchingRule> _ruleThatReturnsFirstHandler;
		private Mock<IMatchingRule> _ruleThatReturnsNoHandlers;
		private RequestHandlerFactory _requestHandlerFactory;

		[SetUp]
		public void SetUp() {
			_dataProducer = new Mock<IDataProducer>();
			_httpResponseDelegate = new Mock<IHttpResponseDelegate>();
			_ruleThatReturnsFirstHandler = new Mock<IMatchingRule>();
			_ruleThatReturnsNoHandlers = new Mock<IMatchingRule>();
			
			_processor = new RequestProcessor(_ruleThatReturnsFirstHandler.Object, new RequestHandlerList());
			_requestHandlerFactory = new RequestHandlerFactory(_processor);

			_ruleThatReturnsFirstHandler
				.SetupSequence(x => x.IsEndpointMatch(It.IsAny<RequestHandler>(), It.IsAny<HttpRequestHead>()))
				.Returns(true)
				.Returns(false);

		}

		[Test]
		public void Get_should_return_handler_with_get_method_set() {
			RequestHandler requestHandler = _requestHandlerFactory.Get("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("GET"));
		}

		[Test]
		public void Post_should_return_handler_with_post_method_set() {
			RequestHandler requestHandler = _requestHandlerFactory.Post("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("POST"));
		}

		[Test]
		public void Put_should_return_handler_with_put_method_set() {
			RequestHandler requestHandler = _requestHandlerFactory.Put("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("PUT"));
		}

		[Test]
		public void Delete_should_return_handler_with_delete_method_set() {
			RequestHandler requestHandler = _requestHandlerFactory.Delete("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("DELETE"));
		}

		[Test]
		public void Head_should_return_handler_with_head_method_set() {
			RequestHandler requestHandler = _requestHandlerFactory.Head("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("HEAD"));
		}

		[Test]
		public void Custom_verb_should_return_handler_with_custom_method_set() {
			RequestHandler requestHandler = _requestHandlerFactory.CustomVerb("nowhere", "PURGE");
			Assert.That(requestHandler.Method, Is.EqualTo("PURGE"));
		}

		[Test]
		public void If_no_handlers_found_should_fire_onresponse_with_a_404() {
			_processor = new RequestProcessor(_ruleThatReturnsNoHandlers.Object, new RequestHandlerList());

			_processor.Add(_requestHandlerFactory.Get("test"));
			_processor.OnRequest(new HttpRequestHead(), _dataProducer.Object, _httpResponseDelegate.Object);
			_httpResponseDelegate.Verify(x => x.OnResponse(It.Is<HttpResponseHead>(y => y.Status == "404 NotFound"),It.Is<IDataProducer>( p => p ==null) ));
		}

		[Test]
		public void If_a_handler_found_should_fire_onresponse_with_that_repsonse() {
			_processor = new RequestProcessor(_ruleThatReturnsFirstHandler.Object, new RequestHandlerList());

			RequestHandler requestHandler = _requestHandlerFactory.Get("test");
			_processor.Add(requestHandler);
			Dictionary<string, string> headers = new Dictionary<string, string>();
			_processor.OnRequest(new HttpRequestHead{ Headers =  headers}, _dataProducer.Object, _httpResponseDelegate.Object);

			_httpResponseDelegate.Verify(
				x => x.OnResponse(requestHandler.ResponseBuilder.BuildHeaders(), 
				requestHandler.ResponseBuilder.BuildBody(headers)));
		}
		
		[Test]
		public void Matching_HEAD_handler_should_output_handlers_expected_response_with_null_body() {

			_processor = new RequestProcessor(_ruleThatReturnsFirstHandler.Object, new RequestHandlerList());

			RequestHandler requestHandler = _requestHandlerFactory.Head("test");
			_processor.Add(requestHandler);
			var httpRequestHead = new HttpRequestHead { Method = "HEAD", Headers = new Dictionary<string, string>() };
			_processor.OnRequest(httpRequestHead, _dataProducer.Object, _httpResponseDelegate.Object);

			_httpResponseDelegate.Verify(x => x.OnResponse(requestHandler.ResponseBuilder.BuildHeaders(), null));
		}

		[Test]
		public void When_a_handler_is_added_should_be_able_to_find_it() {
			string expectedPath = "/blah/test";
			string expectedMethod = "GET";

			var requestProcessor = new RequestProcessor(null, new RequestHandlerList());

			requestProcessor.Add(_requestHandlerFactory.Get(expectedPath));

			var handler = (RequestHandler) requestProcessor.FindHandler(expectedMethod, expectedPath);

			Assert.That(handler.Path, Is.EqualTo(expectedPath));
			Assert.That(handler.Method, Is.EqualTo(expectedMethod));

		}

		[Test]
		public void When_a_handler_is_hit_handlers_request_count_is_incremented() {

			string expectedPath = "/blah/test";
			string expectedMethod = "GET";

			var requestProcessor = new RequestProcessor(_ruleThatReturnsFirstHandler.Object, new RequestHandlerList());

			requestProcessor.Add(_requestHandlerFactory.Get(expectedPath));
			var httpRequestHead = new HttpRequestHead { Headers = new Dictionary<string, string>() };
			httpRequestHead.Path = expectedPath;
			httpRequestHead.Method = expectedPath;
			requestProcessor.OnRequest(httpRequestHead, _dataProducer.Object, _httpResponseDelegate.Object);

			var handler = requestProcessor.FindHandler(expectedMethod, expectedPath);
			Assert.That(handler.RequestCount(), Is.EqualTo(1));
		}

        [Test]
        public void Returns_mock_not_found_when_handler_constraints_cannot_be_verified()
        {
            var excludePhrase = "OhMyDaysssss";

            var handlerWithConstraints = new RequestHandler("", null);
            handlerWithConstraints.WithUrlConstraint(uri => uri.Contains(excludePhrase) == false);

            var matchingRule = new Mock<IMatchingRule>();
            matchingRule
	            .Setup(m => m.IsEndpointMatch(It.IsAny<RequestHandler>(), It.IsAny<HttpRequestHead>()))
	            .Returns(true);
            
            var p = new RequestProcessor(matchingRule.Object, new RequestHandlerList { handlerWithConstraints });

            var response = new Mock<IHttpResponseDelegate>();
            p.OnRequest(new HttpRequestHead{Uri = "http://blah.com/cheese/" + excludePhrase}, null, response.Object);

            var notFoundResponse = (HttpResponseHead) response.Invocations.First().Arguments.First();

            Assert.That(notFoundResponse.Status, Is.EqualTo( "404 NotFound"));
        }
	}
}
