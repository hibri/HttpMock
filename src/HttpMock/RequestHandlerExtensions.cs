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

		public static RequestHandler AsXmlContent(this RequestHandler requestHandler) {
			return requestHandler.AsContentType("text/xml");
		}

		public static RequestHandler AsContentType(this RequestHandler requestHandler, string contentType) {
			requestHandler.ResponseBuilder.WithContentType(contentType);
			return requestHandler;
		}

		public static RequestHandler AddHeader(this RequestHandler requestHandler, string header, string headerValue) {
			requestHandler.ResponseBuilder.AddHeader(header, headerValue);
			return requestHandler;
		}


	}
}