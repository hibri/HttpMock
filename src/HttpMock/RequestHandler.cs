using System.Net;
using Kayak;

namespace HttpMock
{
	public class RequestHandler
	{
		private HttpStatusCode _responseStatusCode;
		private WebAppResponseBuilder _webResponseBuilder = new WebAppResponseBuilder();
		public string Path { get; set; }
		public RequestProcessor RequestProcessor { get; set; }
		public KayakScheduler Scheduler { get; set; }

		public HttpStatusCode ResponseStatusCode {
			get { return _responseStatusCode; }
		}

		public WebAppResponseBuilder ResponseBuilder {
			get { return _webResponseBuilder; }
		}

		public RequestHandler(string path, RequestProcessor requestProcessor) {
			Path = path;
			RequestProcessor = requestProcessor;
		}



		public void SetStatusCode(HttpStatusCode httpStatusCode) {
			_responseStatusCode = httpStatusCode;
		}
	}
}