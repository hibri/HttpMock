using System;
using Kayak;
using Kayak.Http;
using NUnit.Framework;
using Rhino.Mocks;

namespace HttpMock.Unit.Tests {
	[TestFixture]
	public class RequestProcessorTests
	{
		private RequestProcessor _processor;
		private IDataProducer _dataProducer;
		private IHttpResponseDelegate _httpResponseDelegate;
		private IMatchingRule _ruleThatReturnsFirstHandler;
		private IMatchingRule _ruleThatReturnsNoHandlers;
		private IStubResponse _defaultResponse;

		[SetUp]
		public void SetUp() {
			_processor = new RequestProcessor();
			_dataProducer = MockRepository.GenerateStub<IDataProducer>();
			_httpResponseDelegate = MockRepository.GenerateStub<IHttpResponseDelegate>();

			_defaultResponse = MockRepository.GenerateStub<IStubResponse>();

			_ruleThatReturnsFirstHandler = MockRepository.GenerateStub<IMatchingRule>();
			_ruleThatReturnsFirstHandler.Stub(x => x.IsEndpointMatch(null, new HttpRequestHead())).IgnoreArguments().Return(true).Repeat.Once();

			_ruleThatReturnsNoHandlers = MockRepository.GenerateStub<IMatchingRule>();
			_ruleThatReturnsNoHandlers.Stub(x => x.IsEndpointMatch(null, new HttpRequestHead())).IgnoreArguments().Return(false);
		}

		[Test]
		public void Get_should_return_handler_with_get_method_set() {
			RequestHandler requestHandler = _processor.Get("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("GET"));
		}

		[Test]
		public void Post_should_return_handler_with_post_method_set() {
			RequestHandler requestHandler = _processor.Post("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("POST"));
		}

		[Test]
		public void Put_should_return_handler_with_put_method_set() {
			RequestHandler requestHandler = _processor.Put("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("PUT"));
		}

		[Test]
		public void Delete_should_return_handler_with_delete_method_set() {
			RequestHandler requestHandler = _processor.Delete("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("DELETE"));
		}

		[Test]
		public void Head_should_return_handler_with_head_method_set() {
			RequestHandler requestHandler = _processor.Head("nowhere");
			Assert.That(requestHandler.Method, Is.EqualTo("HEAD"));
		}

		[Test]
		public void OnRequest_should_throw_applicationexception_if_no_handlers_supplied() {
			var applicationException = Assert.Throws<ApplicationException>(() => _processor.OnRequest(new HttpRequestHead(), _dataProducer, _httpResponseDelegate));

			Assert.That(applicationException.Message, Is.EqualTo("No handlers have been set up, why do I even bother"));
		}

		[Test]
		public void If_no_handlers_found_should_fire_onresponse_with_default_response() {

			var expectedResponseBuilder = new ResponseBuilder();

			_defaultResponse.Stub(x => x.Get(new HttpRequestHead())).IgnoreArguments().Return(expectedResponseBuilder);

			_processor = new RequestProcessor(_defaultResponse, _ruleThatReturnsNoHandlers);

			_processor.Add(_processor.Get("test"));
			_processor.OnRequest(new HttpRequestHead(), _dataProducer, _httpResponseDelegate);

			_httpResponseDelegate.AssertWasCalled(x => x.OnResponse(expectedResponseBuilder.BuildHeaders(), expectedResponseBuilder.BuildBody()));
		}

		[Test]
		public void If_a_handler_found_should_fire_onresponse_with_that_repsonse() {
			_processor = new RequestProcessor(_defaultResponse, _ruleThatReturnsFirstHandler);

			RequestHandler requestHandler = _processor.Get("test");
			_processor.Add(requestHandler);
			_processor.OnRequest(new HttpRequestHead(), _dataProducer, _httpResponseDelegate);

			_httpResponseDelegate.AssertWasCalled(x => x.OnResponse(requestHandler.ResponseBuilder.BuildHeaders(), requestHandler.ResponseBuilder.BuildBody()));
		}
		
		[Test]
		public void Matching_HEAD_handler_should_output_handlers_expected_response_with_null_body() {

			_processor = new RequestProcessor(_defaultResponse, _ruleThatReturnsFirstHandler);

			RequestHandler requestHandler = _processor.Head("test");
			_processor.Add(requestHandler);
			var httpRequestHead = new HttpRequestHead {Method = "HEAD"};
			_processor.OnRequest(httpRequestHead, _dataProducer, _httpResponseDelegate);

			_httpResponseDelegate.AssertWasCalled(x => x.OnResponse(requestHandler.ResponseBuilder.BuildHeaders(), null));
		}

		[Test]
		public void Unmatching_HEAD_handler_should_output_defaultresponse_with_null_body() {

			var expectedResponseBuilder = new ResponseBuilder();

			var httpRequestHead = new HttpRequestHead {Method = "HEAD"};
			_defaultResponse.Stub(x => x.Get(httpRequestHead)).IgnoreArguments().Return(expectedResponseBuilder);

			_processor = new RequestProcessor(_defaultResponse, _ruleThatReturnsNoHandlers);

			RequestHandler requestHandler = _processor.Head("test");
			_processor.Add(requestHandler);
			_processor.OnRequest(httpRequestHead, _dataProducer, _httpResponseDelegate);

			_httpResponseDelegate.AssertWasCalled(x => x.OnResponse(expectedResponseBuilder.BuildHeaders(), null));
		}
	}
}
