using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
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

			Debug.WriteLine("Processing :" + request.Uri);
			
			RequestHandler handler = _handlers.Where(x => MatchPath( x.Key, request.Uri)).FirstOrDefault().Value;
			if (handler != null) {
				response.OnResponse(handler.ResponseBuilder.BuildHeaders(), handler.ResponseBuilder.BuildBody());
			}
			else {
				ResponseBuilder stubNotFoundResponseBuilder = GetStubNotFoundResponse(request);
				response.OnResponse(stubNotFoundResponseBuilder.BuildHeaders(), stubNotFoundResponseBuilder.BuildBody());
			}
		}

		private ResponseBuilder GetStubNotFoundResponse(HttpRequestHead request) {
			ResponseBuilder stubNotFoundResponseBuilder = new ResponseBuilder();
			stubNotFoundResponseBuilder.Return(string.Format("Stub not found for {0} : {1}", request.Method, request.Uri));
			stubNotFoundResponseBuilder.WithStatus(HttpStatusCode.NotFound);
			return stubNotFoundResponseBuilder;
		}

		private bool MatchPath(string path, string requestUri) {
			return requestUri.StartsWith(path);
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
			string cleanedPath = _applicationPath + path;
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
	}
}