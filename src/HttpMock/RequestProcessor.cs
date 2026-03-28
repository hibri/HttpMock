using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HttpMock
{
	public class RequestProcessor :  IRequestProcessor
	{
		private static readonly ILog _log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
	    private IRequestHandlerList _handlers;
	    private readonly RequestMatcher _requestMatcher;

	    public RequestProcessor(IMatchingRule matchingRule, IRequestHandlerList requestHandlers) {
	        _handlers = requestHandlers;
		    _requestMatcher = new RequestMatcher(matchingRule);
		}

		public void OnRequest(IHttpRequestHead request, Stream requestBody, Action<HttpMockResponseHead, byte[]> respond) {
			_log.DebugFormat("Start Processing request for : {0}:{1}", request.Method, request.Uri);
			if (GetHandlerCount() < 1) {
				ReturnHttpMockNotFound(respond);
				return;
			}

			var handler = _requestMatcher.Match(request, _handlers);

			if (handler == null) {
				_log.DebugFormat("No Handlers matched");
				ReturnHttpMockNotFound(respond);
				return;
			}
			HandleRequest(request, requestBody, respond, handler);
		}

	    private static async void HandleRequest(IHttpRequestHead request, Stream requestBody, Action<HttpMockResponseHead, byte[]> respond, IRequestHandler handler)
	    {
	        _log.DebugFormat("Matched a handler {0}:{1} {2}", handler.Method, handler.Path, DumpQueryParams(handler.QueryParams));

            if (handler.ResponseDelay > TimeSpan.Zero)
            {
                await Task.Delay(handler.ResponseDelay);
            }

	        byte[] responseBody = request.Method != "HEAD"
	            ? handler.ResponseBuilder.BuildBody(request.Headers)
	            : null;

	        if (request.HasBody() && requestBody != null)
	        {
	            try
	            {
	                using (var reader = new StreamReader(requestBody, Encoding.UTF8, false, 4096, leaveOpen: true))
	                {
	                    string bufferedBody = reader.ReadToEnd();
	                    handler.RecordRequest(request, bufferedBody);
	                    _log.DebugFormat("Body: {0}", bufferedBody);
	                }
	            }
	            catch (Exception error)
	            {
	                _log.DebugFormat("Error while reading body {0}", error.Message);
	                handler.RecordRequest(request, null);
	            }
	        }
	        else
	        {
	            handler.RecordRequest(request, null);
	        }

	        respond(handler.ResponseBuilder.BuildHeaders(), responseBody);
	        _log.DebugFormat("End Processing request for : {0}:{1}", request.Method, request.Uri);
	    }

		private int GetHandlerCount() {
			return _handlers.Count();
		}

	    public IRequestVerify FindHandler(string method, string path) {
			return (IRequestVerify) _handlers.Where(x => x.Path == path && x.Method == method).FirstOrDefault();
		}

		private static string DumpQueryParams(IDictionary<string, string> queryParams) {
			var sb = new StringBuilder();
			foreach (var param in queryParams) {
				sb.AppendFormat("{0}={1}&", param.Key, param.Value);
			}
			return sb.ToString();
		}

		private static void ReturnHttpMockNotFound(Action<HttpMockResponseHead, byte[]> respond) {
			var dictionary = new Dictionary<string, string>
			{
				{HttpHeaderNames.ContentLength, "0"},
				{"X-HttpMockError", "No handler found to handle request"}
			};

			var notFoundResponse = new HttpMockResponseHead
			{
				Status = string.Format("{0} {1}", 404, "NotFound"),
				Headers = dictionary
			};
			respond(notFoundResponse, null);
		}

		public void ClearHandlers() {
			_handlers = new RequestHandlerList();
		}

		public void Add(RequestHandler requestHandler) {
			_handlers.Add(requestHandler);
		}

		public string WhatDoIHave() {
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("Handlers:");
			foreach (RequestHandler handler in _handlers) {
				stringBuilder.Append(handler.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}