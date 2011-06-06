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
		private Dictionary<string, RequestHandler> _handlers;
		private IDisposable _closeObject;

		public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response) {

			string pathToMatch = request.Uri.Remove(request.Uri.IndexOf(_applicationPath, 0), _applicationPath.Length).Replace("/", "");
			RequestHandler handler = _handlers.Where(x => x.Key.Equals(pathToMatch)).FirstOrDefault().Value;
			if (handler != null) {

				var headers = handler.ResponseBuilder.BuildHeaders();
				var responseBody = handler.ResponseBuilder.BuildBody();
				response.OnResponse(headers, responseBody);

			}
			else {
				ResponseBuilder stubNotFoundResponseBuilder = new ResponseBuilder();
				stubNotFoundResponseBuilder.WithBody(string.Format("Stub not found for {0} : {1}", request.Method, request.Uri));
				HttpResponseHead headers = stubNotFoundResponseBuilder.BuildHeaders();
				var responseBody = stubNotFoundResponseBuilder.BuildBody();
				response.OnResponse(headers, responseBody);
			}
		}

		public RequestHandler Get(string path) {
			return AddHandler(path);
		}

		public RequestHandler Post(string path) {
			return AddHandler(path);
		}

		public RequestHandler Put(string path)
		{
			return AddHandler(path);
		}

		public RequestHandler Delete(string path)
		{
			return AddHandler(path);
		}

		private RequestHandler AddHandler(string path) {
			string cleanedPath = path.Replace("/","");
			var requestHandler = new RequestHandler(cleanedPath, this);

			return requestHandler;
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
			_applicationPath = baseUri;
			if (_applicationPath == "/")
				_applicationPath = "";
		}
	}
}