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
		private Dictionary<string, RequestHandler> _handlers = new Dictionary<string, RequestHandler>();
		private IDisposable _closeObject;
		
		public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response) {

			if(_handlers.Count() < 1)
				throw new ApplicationException("No handlers have been set up, why do I even bother");
			
			RequestHandler handler = _handlers.Where(x => MatchPath( x.Key, request.Uri)).FirstOrDefault().Value;
			if (handler != null) {
				response.OnResponse(handler.ResponseBuilder.BuildHeaders(), handler.ResponseBuilder.BuildBody());
			}
			else {
				ResponseBuilder stubNotFoundResponseBuilder = new StubNotFoundResponse().Get(request);
				response.OnResponse(stubNotFoundResponseBuilder.BuildHeaders(), stubNotFoundResponseBuilder.BuildBody());
			}
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
			_handlers = new Dictionary<string, RequestHandler>();
		}

		public void Add(RequestHandler requestHandler) {
			_handlers.Add(requestHandler.Path, requestHandler);
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

		private bool MatchPath(string path, string requestUri) {
			return requestUri.StartsWith(path);
		}
	}
}