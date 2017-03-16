using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace HttpMock.Verify.NUnit
{
	public static  class RequestHandlerExpectExtensions
	{
		public static void WithBody(this IRequestVerify handler, string expectedBody) {
			
			Assert.That(handler.GetBody(), Is.EqualTo(expectedBody));
		}

		public static void WithBody(this IRequestVerify handler, IResolveConstraint constraint)
		{
			Assert.That(handler.GetBody(), constraint.Resolve());
		}

		public static void Times(this IRequestVerify handler, int times)
		{
			Assert.That(handler.RequestCount(), Is.EqualTo(times));
		}

		public static void WithHeader(this IRequestVerify handler, string header, IResolveConstraint match)
		{
			string headerValue;
			handler.LastRequest().RequestHead.Headers.TryGetValue(header, out headerValue);
			Assert.That(headerValue, Is.Not.Null, "Request did not contain a header '{0}'", header);
			Assert.That(headerValue, match);
		}
	}
}