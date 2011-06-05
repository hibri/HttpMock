using System.Net;
using Kayak;

namespace HttpMock
{
	public class RequestHandler
	{
		private readonly ResponseBuilder _webResponseBuilder = new ResponseBuilder();

		public RequestHandler(string path, RequestProcessor requestProcessor) {
			Path = path;
			RequestProcessor = requestProcessor;
		}

		public string Path { get; set; }
		public RequestProcessor RequestProcessor { get; set; }
		public KayakScheduler Scheduler { get; set; }

		public ResponseBuilder ResponseBuilder {
			get { return _webResponseBuilder; }
		}
	}
}