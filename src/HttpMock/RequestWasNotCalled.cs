using NUnit.Framework;

namespace HttpMock
{
	public class RequestWasNotCalled
	{
		private readonly IRequestProcessor _requestProcessor;

		public RequestWasNotCalled(IRequestProcessor requestProcessor) {
			_requestProcessor = requestProcessor;
		}

		public IRequestVerify Get(string path)
		{
			return AssertHandler( "GET", path);
		}

		private IRequestVerify AssertHandler(string method, string path) {
			var handler = _requestProcessor.FindHandler(method, path);
			if (handler != null) {
				Assert.That(handler.RequestCount(), Is.EqualTo(0), "Expected not to find a request for {1}{0} but was found", method, path);
			}else {
				Assert.That(handler, Is.Null, "Expected not to find a request for {1}{0} but was found", method, path);
			}
			return handler;
		}

		public IRequestVerify Post(string path) {
			return CustomVerb("POST", path);
		}

		public IRequestVerify Put(string path)
		{
			return CustomVerb("PUT", path);
		}

		public IRequestVerify Delete(string path)
		{
			return CustomVerb( "DELETE", path);
		}

		public IRequestVerify Head(string path)
		{
			return CustomVerb( "HEAD", path);
		}

		public IRequestVerify CustomVerb(string verb, string path)
		{
			return AssertHandler(verb, path);
		}
	}
}