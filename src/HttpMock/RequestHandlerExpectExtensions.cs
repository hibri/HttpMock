
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
	}
}