
using System;

namespace HttpMock
{
	public static  class RequestHandlerExpectExtensions
	{
		public static void WithBody(this RequestHandler handler, string expectedBody) {
			HttpMockAssert.IsEqual( handler.GetBody(), expectedBody, null );
		}

		public static void WithBody( this RequestHandler handler, Action<string> doAssert) {
			doAssert( handler.GetBody() );
		}

		public static void Times(this RequestHandler handler, int times)
		{
			HttpMockAssert.IsEqual(handler.RequestCount(), times, null);
		}

		public static void WithHeader( this RequestHandler handler, string header, Action<string> doAssert ) {
			string headerValue;
			handler.LastRequest().RequestHead.Headers.TryGetValue( header, out headerValue );
			HttpMockAssert.IsNotNull( headerValue, string.Format( "Request did not contain a header '{0}'", header ) );
			doAssert( headerValue );
		}
	}
}