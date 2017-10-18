using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
    public static class TestContextExtension
    {
        private const string HostKey = "HostURL";
        private const string EndpointToHitKey = "EndpointToHit";
        private const string HttpMockKey = "HttpMock";

        public static string GetCurrentHostUrl(this TestContext testContext)
        {
            return (string)testContext.Test.Properties.Get(HostKey);
        }

        public static void SetCurrentHostUrl(this TestContext testContext, string hostUrl)
        {
            testContext.Test.Properties.Add(HostKey, hostUrl);
        }

        public static string GetCurrentEndpointToHit(this TestContext testContext)
        {
            return (string)testContext.Test.Properties.Get(EndpointToHitKey);
        }

        public static void SetCurrentEndpointToHit(this TestContext testContext, string endpointToHit)
        {
            testContext.Test.Properties.Add(EndpointToHitKey, endpointToHit);
        }


        public static IHttpServer GetCurrentHttpMock(this TestContext testContext)
        {
            return (IHttpServer)testContext.Test.Properties.Get(HttpMockKey);
        }

        public static void SetCurrentHttpMock(this TestContext testContext, IHttpServer httpMock)
        {
            testContext.Test.Properties.Add(HttpMockKey, httpMock);
        }
    }
}
