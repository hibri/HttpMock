using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	public class RequestProcessor :  IRequestProcessor
	{
		private static readonly ILogger<RequestProcessor> _log = HttpMockLogging.CreateLogger<RequestProcessor>();
	    private IRequestHandlerList _handlers;
	    private readonly RequestMatcher _requestMatcher;

	    public RequestProcessor(IMatchingRule matchingRule, IRequestHandlerList requestHandlers) {
	        _handlers = requestHandlers;
		    _requestMatcher = new RequestMatcher(matchingRule);
		}

		public void OnRequest(IHttpRequestHead request, Stream requestBody, Action<HttpMockResponseHead, byte[]> respond) {
			_log.LogDebug("Start Processing request for : {Method}:{Uri}", SanitizeForLog(request.Method), SanitizeForLog(request.Uri));
			if (GetHandlerCount() < 1) {
				ReturnHttpMockNotFound(respond);
				return;
			}

			var handler = _requestMatcher.Match(request, _handlers);

			if (handler == null) {
				_log.LogDebug("No Handlers matched");
				ReturnHttpMockNotFound(respond);
				return;
			}
			_ = HandleRequest(request, requestBody, respond, handler);
		}

	    private static async Task HandleRequest(IHttpRequestHead request, Stream requestBody, Action<HttpMockResponseHead, byte[]> respond, IRequestHandler handler)
	    {
	        _log.LogDebug("Matched a handler {Method}:{Path} {QueryParams}", handler.Method, handler.Path, DumpQueryParams(handler.QueryParams));

            if (handler.ResponseDelay > TimeSpan.Zero)
            {
                await Task.Delay(handler.ResponseDelay);
            }

	        byte[] responseBody = request.Method != "HEAD"
	            ? handler.ResponseBuilder.BuildBody(request.Headers)
	            : null;

	        if (request.HasEntityBody && requestBody != null)
	        {
	            try
	            {
	                using (var reader = new StreamReader(requestBody, Encoding.UTF8, false, 4096, leaveOpen: true))
	                {
	                    string bufferedBody = reader.ReadToEnd();
	                    handler.RecordRequest(request, bufferedBody);
	                    _log.LogDebug("Body: {Body}", SanitizeForLog(bufferedBody));
	                }
	            }
	            catch (Exception error)
	            {
	                _log.LogDebug("Error while reading body {ErrorMessage}", error.Message);
	                handler.RecordRequest(request, null);
	            }
	        }
	        else
	        {
	            handler.RecordRequest(request, null);
	        }

	        respond(handler.ResponseBuilder.BuildHeaders(), responseBody);
	        _log.LogDebug("End Processing request for : {Method}:{Uri}", SanitizeForLog(request.Method), SanitizeForLog(request.Uri));
	    }

		private static string SanitizeForLog(string value) =>
			value?.Replace("\r", "\\r").Replace("\n", "\\n");

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