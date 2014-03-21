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
			return AssertHandler( "GET", path);
		}

		private RequestHandler AssertHandler(string method, string path) {
			var handler = _requestProcessor.FindHandler(method, path);
			if (handler != null) {
				Assert.That(handler.RequestCount(), Is.EqualTo(0), "Expected not to find a request for {1}{0} but was found", method, path);
			}else {
				Assert.That(handler, Is.Null, "Expected not to find a request for {1}{0} but was found", method, path);
			}
			return handler;
		}

		public RequestHandler Post(string path) {
			return CustomVerb("POST", path);
		}

		public RequestHandler Put(string path)
		{
			return CustomVerb("PUT", path);
		}

		public RequestHandler Delete(string path)
		{
			return CustomVerb( "DELETE", path);
		}

		public RequestHandler Head(string path)
		{
			return CustomVerb( "HEAD", path);
		}

		public RequestHandler CustomVerb(string verb, string path)
		{
			return AssertHandler(verb, path);
		}
	}
}