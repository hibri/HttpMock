using System;
using FakeItEasy;
using Kayak;
using Kayak.Http;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class RequestProcessorTests
	{
		[Test]
		public void Should_create_a_handler_for_the_given_path()
		{
			var expectedPath = "/somepath";

			RequestProcessor requestProcessor = new RequestProcessor();

			Assert.That(requestProcessor.Get(expectedPath).Path, Is.EqualTo(expectedPath));
			Assert.That(requestProcessor.Post(expectedPath).Path, Is.EqualTo(expectedPath));
			Assert.That(requestProcessor.Put(expectedPath).Path, Is.EqualTo(expectedPath));
			Assert.That(requestProcessor.Delete(expectedPath).Path, Is.EqualTo(expectedPath));
		}

		[Test]
		public void Should_match()
		{
			var requestPath = "/path";
			FakeResponseDelegate response = new FakeResponseDelegate();

			HttpRequestHead request = new HttpRequestHead();
			request.Uri = requestPath;

			IDataProducer body = A.Fake<IDataProducer>();
			RequestProcessor requestProcessor = new RequestProcessor();
			requestProcessor.ClearHandlers();
			requestProcessor.Get(requestPath).OK();
			

			requestProcessor.OnRequest(request, body, response);


			Assert.That(response.Head.Status, Is.EqualTo("200 OK"));
		}

		[Test]
		public void When_a_path_is_not_matched()
		{
			var requestPath = "/path";
			FakeResponseDelegate response = new FakeResponseDelegate();

			HttpRequestHead request = new HttpRequestHead();
			request.Uri = requestPath;
			IDataProducer body = A.Fake<IDataProducer>();
			RequestProcessor requestProcessor = new RequestProcessor();
			requestProcessor.ClearHandlers();
			requestProcessor.Get("/someotherpath").OK();


			requestProcessor.OnRequest(request, body, response);


			Assert.That(response.Head.Status, Is.EqualTo("404 NotFound"));
		}
	}
	public class FakeResponseDelegate : IHttpResponseDelegate
	{
		private IDataProducer _body;
		private HttpResponseHead _head;

		public HttpResponseHead Head {
			get { return _head; }
		}

		public void OnResponse(HttpResponseHead head, IDataProducer body) {
			_head = head;
			_body = body;
		}
	}
	}
