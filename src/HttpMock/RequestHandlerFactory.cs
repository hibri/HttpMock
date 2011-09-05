using System.Collections.Generic;

namespace HttpMock
{
	public class RequestHandlerFactory
	{
		private readonly RequestProcessor _requestProcessor;

		public RequestHandlerFactory(RequestProcessor requestProcessor) {
			_requestProcessor = requestProcessor;
		}

		public RequestHandler Get(string path) {
			return CreateHandler(path, "GET");
		}

		public RequestHandler Post(string path) {
			return CreateHandler(path, "POST");
		}

		public RequestHandler Put(string path) {
			return CreateHandler(path, "PUT");
		}

		public RequestHandler Delete(string path) {
			return CreateHandler(path, "DELETE");
		}

		public RequestHandler Head(string path) {
			return CreateHandler(path, "HEAD");
		}

		public void ClearHandlers() {
			new List<RequestHandler>();
		}



		private RequestHandler CreateHandler(string path, string method) {
			string cleanedPath = path;
			var requestHandler = new RequestHandler(cleanedPath, _requestProcessor) {Method = method};
			return requestHandler;
		}
	}

}