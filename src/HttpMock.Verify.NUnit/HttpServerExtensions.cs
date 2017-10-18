using System;

namespace HttpMock.Verify.NUnit
{
    public static class HttpServerExtensions
    {


        public static IRequestVerify AssertWasCalled(this IHttpServer httpServer, Func<RequestWasCalled, IRequestVerify> func, Guid sessionId = default(Guid))
        {
            return func.Invoke(new RequestWasCalled(httpServer.GetRequestProcessor(), sessionId));
        }

        public static IRequestVerify AssertWasNotCalled(this IHttpServer httpServer, Func<RequestWasNotCalled, IRequestVerify> func, Guid sessionId = default(Guid))
        {
            return func.Invoke(new RequestWasNotCalled(httpServer.GetRequestProcessor(), sessionId));
        }
    }
}