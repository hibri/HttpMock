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
		public string Method { get; set; }
		public RequestProcessor RequestProcessor { get; set; }
		public KayakScheduler Scheduler { get; set; }

		public ResponseBuilder ResponseBuilder {
			get { return _webResponseBuilder; }
		}

		public RequestHandler Return( string responseBody) {

			_webResponseBuilder.Return(responseBody);
			return this;
		}

		public RequestHandler ReturnFile(string pathToFile)
		{
			_webResponseBuilder.WithFile(pathToFile);
			return this;
		}
	}
}