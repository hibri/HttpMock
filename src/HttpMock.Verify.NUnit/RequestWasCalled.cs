using System;
using NUnit.Framework;

namespace HttpMock.Verify.NUnit
{
	public class RequestWasCalled
	{
		private readonly IRequestProcessor _requestProcessor;

		public RequestWasCalled(IRequestProcessor requestProcessor) {
			_requestProcessor = requestProcessor;
		}

		public IRequestVerify Get(string path)
		{
			return AssertHandler("GET", path);
		}

		private IRequestVerify AssertHandler(string method, string path, Guid sessionId=default(Guid)) {
			var handler = _requestProcessor.FindHandler(method, path, sessionId);
			Assert.That(handler, Is.Not.Null, string.Format("Handler for path {0} and method {1} was not stubbed", path, method));
			Assert.That(handler.RequestCount(), Is.GreaterThan(0), string.Format("Handler for path {0} and method {1} was never called", path, method));
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
			return CustomVerb("DELETE", path);
		}

		public IRequestVerify Head(string path)
		{
			return CustomVerb("HEAD", path);
		}

		public IRequestVerify CustomVerb(string verb, string path)
		{
			return AssertHandler(verb, path);
		}
	}
}