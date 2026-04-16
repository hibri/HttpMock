using System.Net;

namespace HttpMock
{
	public class StubNotFoundResponse : IStubResponse
	{
		public ResponseBuilder Get(IHttpRequestHead request) {
			var stubNotFoundResponseBuilder = new ResponseBuilder();
			stubNotFoundResponseBuilder.Return($"Stub not found for {request.Method} : {request.Uri}");
			stubNotFoundResponseBuilder.WithStatus(HttpStatusCode.NotFound);
			return stubNotFoundResponseBuilder;
		}
	}
}