using System;
using System.Collections.Generic;
using System.Linq;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public class RequestProcessor : IHttpRequestDelegate
	{
		private string _applicationPath;
		private List<RequestHandler> _handlers = new List<RequestHandler>();
		private IDisposable _closeObject;
		private readonly IMatchingRule _matchingRule;

		public RequestProcessor() {
			_matchingRule = new EndpointMatchingRule();
		}

		public RequestProcessor(IMatchingRule matchingRule) {
			_matchingRule = matchingRule;
		}

		public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response) {

			if(_handlers.Count() < 1)
				throw new ApplicationException("No handlers have been set up, why do I even bother");

			RequestHandler handler = _handlers.Where(x => _matchingRule.IsEndpointMatch(x, request)).FirstOrDefault();

			if (handler == null) {
				var dictionary = new Dictionary<string, string>
				{
					{ HttpHeaderNames.ContentLength, "0" }, 
					{"HttpMockError", "StubNotFound"}
				};

				var notFoundResponse = new HttpResponseHead { Status = string.Format("{0} {1}", 404, "NotFound"), Headers = dictionary };
				response.OnResponse(notFoundResponse, null);
				return;
			}

			IDataProducer dataProducer = request.Method != "HEAD" ? handler.ResponseBuilder.BuildBody() : null;
			response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
		}

		public RequestHandler Get(string path) {
			return AddHandler(path, "GET");
		}

		public RequestHandler Post(string path) {
			return AddHandler(path, "POST");
		}

		public RequestHandler Put(string path)
		{
			return AddHandler(path, "PUT");
		}

		public RequestHandler Delete(string path)
		{
			return AddHandler(path, "DELETE");
		}

		public RequestHandler Head(string path) {
			return AddHandler(path, "HEAD");
		}
		
		public void ClearHandlers() {
			_handlers = new List<RequestHandler>();
		}

		public void Add(RequestHandler requestHandler) {
			_handlers.Add(requestHandler);
		}

		public void SetCloseObject(IDisposable closeObject) {
			_closeObject = closeObject;
		}

		public  void Stop() {
			_closeObject.Dispose();
		}

		public void SetBaseUri(string baseUri) {
			if (baseUri.EndsWith("/")) {
				_applicationPath = baseUri.TrimEnd('/');
			}

			else if (!baseUri.StartsWith("/")) {
				_applicationPath = "/" + baseUri;
			}
			else {
				_applicationPath = baseUri;
			}
		}

		private RequestHandler AddHandler(string path, string method) {
			string cleanedPath = _applicationPath + path;
			var requestHandler = new RequestHandler(cleanedPath, this) {Method = method};
			return requestHandler;
		}
	}
}