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
			Assert.That(handler, Is.Not.Null, string.Format("Handler for path {0} and method {1} was not stubbed", path, method));
			Assert.That(handler.RequestCount(), Is.GreaterThan(0), string.Format("Handler for path {0} and method {1} was never called", path, method));
			return handler;
		}

		public RequestHandler Post(string path) {
			return CustomVerb(path, "POST");
		}

		public RequestHandler Put(string path)
		{
			return CustomVerb(path, "PUT");
		}

		public RequestHandler Delete(string path)
		{
			return CustomVerb(path, "DELETE");
		}

		public RequestHandler Head(string path)
		{
			return CustomVerb(path, "HEAD");
		}

		public RequestHandler CustomVerb(string path, string verb)
		{
			return AssertHandler(path, verb);
		}
	}
}