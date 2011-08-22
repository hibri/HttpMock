using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Kayak;
using Kayak.Http;
using log4net;

namespace HttpMock
{
	public class RequestProcessor : IHttpRequestDelegate
	{
		private string _applicationPath;
		private List<RequestHandler> _handlers = new List<RequestHandler>();
		private readonly IMatchingRule _matchingRule;
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public RequestProcessor() {
			_matchingRule = new EndpointMatchingRule();

		}

		public RequestProcessor(IMatchingRule matchingRule) {
			_matchingRule = matchingRule;
		}

		public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response) {
			_log.DebugFormat("Start Processing request for : {0}:{1}", request.Method , request.Uri);
			if (_handlers.Count() < 1) {
				ReturnHttpMockNotFound(response);
				return;
			}

			RequestHandler handler = _handlers.Where(x => _matchingRule.IsEndpointMatch(x, request)).FirstOrDefault();

			if (handler == null) {
				_log.DebugFormat("No Handlers matched");
				ReturnHttpMockNotFound(response);
				return;
			}
			_log.DebugFormat("Matched a handler {0},{1}, {2}", handler.Method, handler.Path , handler.QueryParams);

			IDataProducer dataProducer = request.Method != "HEAD" ? handler.ResponseBuilder.BuildBody() : null;
			response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
			_log.DebugFormat("End Processing request for : {0}:{1}", request.Method, request.Uri);
			return;
		}

		private static void ReturnHttpMockNotFound(IHttpResponseDelegate response) {
			var dictionary = new Dictionary<string, string>
			{
				{ HttpHeaderNames.ContentLength, "0" }, 
				{ "SevenDigital-HttpMockError", "No handler found to handle request" }
			};

			var notFoundResponse = new HttpResponseHead
			{Status = string.Format("{0} {1}", 404, "NotFound"), Headers = dictionary};
			response.OnResponse(notFoundResponse, null);
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

		public string WhatDoIHave()
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Handlers:");
			foreach(var handler in _handlers)
			{
				stringBuilder.Append(handler.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}