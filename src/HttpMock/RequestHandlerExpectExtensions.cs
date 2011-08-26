using System.Net;
using NUnit.Framework;

namespace HttpMock
{
	public static  class RequestHandlerExpectExtensions
	{
		public static void WithBody(this RequestHandler handler, string expectedBody) {
			
			Assert.That(handler.GetBody(), Is.EqualTo(expectedBody));
		}
	}
}