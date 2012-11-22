
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace HttpMock
{
	public static  class RequestHandlerExpectExtensions
	{
		public static void WithBody(this RequestHandler handler, string expectedBody) {
			
			Assert.That(handler.GetBody(), Is.EqualTo(expectedBody));
		}

		public static void WithBody(this RequestHandler handler, IResolveConstraint constraint)
		{
			Assert.That(handler.GetBody(), constraint.Resolve());
		}

		public static void Times(this RequestHandler handler, int times)
		{
			Assert.That(handler.RequestCount(), Is.EqualTo(times));
		}
	}
}