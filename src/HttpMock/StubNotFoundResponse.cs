using System.Net;

namespace HttpMock
{
	public class StubNotFoundResponse : IStubResponse
	{
		public ResponseBuilder Get(IHttpRequestHead request) {
			var stubNotFoundResponseBuilder = new ResponseBuilder();
			stubNotFoundResponseBuilder.Return(string.Format("Stub not found for {0} : {1}", request.Method, request.Uri));
			stubNotFoundResponseBuilder.WithStatus(HttpStatusCode.NotFound);
			return stubNotFoundResponseBuilder;
		}
	}
}