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

		[SetUp]
		public void SetUp() {
			_processor = new RequestProcessor();
			_dataProducer = MockRepository.GenerateStub<IDataProducer>();
			_httpResponseDelegate = MockRepository.GenerateStub<IHttpResponseDelegate>();
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
		public void Matching_handler_should_output_handlers_expected_response() {
			const string expected = "lost";
			var request = new HttpRequestHead {Uri = expected, Method = "GET"};

			RequestHandler requestHandler = _processor.Get(expected);
			_processor.Add(requestHandler);
			_processor.OnRequest(request, _dataProducer, _httpResponseDelegate);

			_httpResponseDelegate.AssertWasCalled(x => x.OnResponse(requestHandler.ResponseBuilder.BuildHeaders(), requestHandler.ResponseBuilder.BuildBody()));
		}

		[Test]
		public void Matching_HEAD_handler_should_output_handlers_expected_response_with_null_body() {
			const string expected = "lost";
			var request = new HttpRequestHead { Uri = expected, Method = "HEAD" };

			RequestHandler requestHandler = _processor.Head(expected);
			_processor.Add(requestHandler);
			_processor.OnRequest(request, _dataProducer, _httpResponseDelegate);

			_httpResponseDelegate.AssertWasCalled(x => x.OnResponse(requestHandler.ResponseBuilder.BuildHeaders(), null));
		}

		[Test]
		public void No_matching_handlers_should_output_stub_not_found_response() {
			var defaultResponse = MockRepository.GenerateStub<IStubResponse>();
			var expectedResponseBuilder = new ResponseBuilder();
			
			defaultResponse.Stub(x => x.Get(new HttpRequestHead())).IgnoreArguments().Return(expectedResponseBuilder);

			_processor = new RequestProcessor(defaultResponse);
			const string uriToMatch = "whatwereallywant";
			const string uriThatDoesNotMatch = "zigazigahhh";

			var actualRequest = new HttpRequestHead { Uri = uriToMatch };

			RequestHandler requestHandler = _processor.Get(uriThatDoesNotMatch);
			_processor.Add(requestHandler);
			_processor.OnRequest(actualRequest, _dataProducer, _httpResponseDelegate);

			_httpResponseDelegate.AssertWasCalled(x => x.OnResponse(expectedResponseBuilder.BuildHeaders(), expectedResponseBuilder.BuildBody()));
		}
	}
}
