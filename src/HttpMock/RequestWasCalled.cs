using NUnit.Framework;

namespace HttpMock
{
	public class RequestWasCalled
	{
		private readonly IRequestProcessor _requestProcessor;

		public RequestWasCalled(IRequestProcessor requestProcessor) {
			_requestProcessor = requestProcessor;
		}

		public RequestHandler Get(string path)
		{
			return AssertHandler(path, "GET");
		}

		private RequestHandler AssertHandler(string method, string path) {
			var handler = _requestProcessor.FindHandler(path, method);
			Assert.That(handler.RequestCount(), Is.GreaterThan(0));
			return handler;
		}

		public RequestHandler Post(string path) {
			return AssertHandler(path, "POST");
		}

		public RequestHandler Put(string path)
		{
			return AssertHandler(path, "PUT");
		}

		public RequestHandler Delete(string path)
		{
			return AssertHandler(path, "DELETE");
		}

		public RequestHandler Head(string path)
		{
			 return AssertHandler(path, "HEAD");
		}

	}
}