using System.Net;

namespace HttpMock
{
	public static  class AppExtensions
	{
		public static void OK(this RequestHandler requestHandler) {
			requestHandler.RequestProcessor.Add(requestHandler, HttpStatusCode.OK);
		}
		public static RequestHandler Return(this RequestHandler requestHandler, string hello) {

			requestHandler.ResponseBuilder.WithBody(hello);
			return requestHandler;
		}
	}
}