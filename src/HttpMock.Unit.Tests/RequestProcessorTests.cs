using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kayak;
using Kayak.Http;
using NUnit.Framework;
using Rhino.Mocks;

namespace HttpMock.Unit.Tests {
	[TestFixture]
	public class RequestProcessorTests
	{
		private RequestProcessor _processor;

		[SetUp]
		public void SetUp() {
			_processor = new RequestProcessor();
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
			var dataProducer = MockRepository.GenerateStub<IDataProducer>();
			var httpResponseDelegate = MockRepository.GenerateStub<IHttpResponseDelegate>();
			var applicationException = Assert.Throws<ApplicationException>(() => _processor.OnRequest(new HttpRequestHead(), dataProducer, httpResponseDelegate));

			Assert.That(applicationException.Message, Is.EqualTo("No handlers have been set up, why do I even bother"));
		}
	}
}
