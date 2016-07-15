
using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;

namespace HttpMock
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

        public static void WithParams(this IRequestVerify handler, IDictionary<string, string> expectedQueryParameters)
        {
            var queryParamsString = handler.LastRequest().RequestHead.QueryString;
            
            Assert.That(queryParamsString, Is.Not.Null, "Request did not contain query parameters");

            var queryParams = queryParamsString.Split('&').ToList()
                .Select(s => s.Split('='))
                .ToDictionary(key => key[0].Trim(), value => value[1].Trim());
            Assert.IsTrue(queryParams.ContentEquals(expectedQueryParameters), "The query parameters {0} do not match the expected ones {1}", queryParams, expectedQueryParameters);
        }
    }
}