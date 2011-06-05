using System.Net;

namespace HttpMock
{
	public static  class RequestHandlerExtensions
	{
		public static void OK(this RequestHandler requestHandler) {
			requestHandler.WithStatus(HttpStatusCode.OK);
		}

		public static void WithStatus(this RequestHandler requestHandler, HttpStatusCode httpStatusCode) {
			requestHandler.ResponseBuilder.WithStatus(httpStatusCode);
			requestHandler.RequestProcessor.Add(requestHandler);
		}

		public static void NotFound(this RequestHandler requestHandler)
		{
			requestHandler.WithStatus(HttpStatusCode.NotFound);
		}
	}
}