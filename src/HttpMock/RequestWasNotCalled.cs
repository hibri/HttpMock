using NUnit.Framework;

namespace HttpMock
{
	public class RequestWasNotCalled
	{
		private readonly IRequestProcessor _requestProcessor;

		public RequestWasNotCalled(IRequestProcessor requestProcessor) {
			_requestProcessor = requestProcessor;
		}

		public RequestHandler Get(string path)
		{
			return AssertHandler(path, "GET");
		}

		private RequestHandler AssertHandler(string method, string path) {
			var handler = _requestProcessor.FindHandler(path, method);
			if (handler != null) {
				Assert.That(handler.RequestCount(), Is.EqualTo(0), "Expected not to find a request for {1}{0} but was found", method, path);
			}else {
				Assert.That(handler, Is.Null, "Expected not to find a request for {1}{0} but was found", method, path);
			}
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