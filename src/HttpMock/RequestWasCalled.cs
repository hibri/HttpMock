//using NUnit.Framework;

namespace HttpMock {

	public class RequestWasCalled {
		private readonly IRequestProcessor _requestProcessor;

		public RequestWasCalled( IRequestProcessor requestProcessor ) {
			_requestProcessor = requestProcessor;
		}

		public RequestHandler Get( string path ) {
			return AssertHandler( "GET", path );
		}

		private RequestHandler AssertHandler( string method, string path ) {
			var handler = _requestProcessor.FindHandler( method, path );
			HttpMockAssert.IsNotNull( handler, string.Format( "Handler for path {0} and method {1} was not stubbed", path, method ) );
			HttpMockAssert.IsGreaterThan( handler.RequestCount(), 0, string.Format( "Handler for path {0} and method {1} was never called", path, method ) );
			return handler;
		}

		public RequestHandler Post( string path ) {
			return CustomVerb( "POST", path );
		}

		public RequestHandler Put( string path ) {
			return CustomVerb( "PUT", path );
		}

		public RequestHandler Delete( string path ) {
			return CustomVerb( "DELETE", path );
		}

		public RequestHandler Head( string path ) {
			return CustomVerb( "HEAD", path );
		}

		public RequestHandler CustomVerb( string verb, string path ) {
			return AssertHandler( verb, path );
		}
	}
}