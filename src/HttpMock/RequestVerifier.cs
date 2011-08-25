using NUnit.Framework;

namespace HttpMock
{
	public class RequestVerifier
	{
		private readonly IRequestProcessor _requestProcessor;

		public RequestVerifier(IRequestProcessor requestProcessor) {
			_requestProcessor = requestProcessor;
		}

		public RequestHandler Get(string path)
		{
			AssertHandler(path, "GET");
			return null;
		}

		private void AssertHandler(string path, string method) {
			var handler = _requestProcessor.FindHandler(path, method);
			Assert.That(handler.RequestCount(), Is.GreaterThan(0));
		}

		public RequestHandler Post(string path) {
			AssertHandler(path, "POST");
			return null;
		}

		public RequestHandler Put(string path)
		{
			AssertHandler(path, "PUT");
			return null;
		}

		public RequestHandler Delete(string path)
		{
			AssertHandler(path, "DELETE");
			return null;
		}

		public RequestHandler Head(string path)
		{
			 AssertHandler(path, "HEAD");
			 return null;
		}

	}
}