using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	public class RequestProcessor :  IRequestProcessor
	{
		private const string NotFoundStatusCode = "404";
		private readonly ILogger<RequestProcessor> _log;
	    private IRequestHandlerList _handlers;
	    private readonly RequestMatcher _requestMatcher;

	    public RequestProcessor(IMatchingRule matchingRule, IRequestHandlerList requestHandlers, ILoggerFactory loggerFactory = null) {
	        _handlers = requestHandlers;
		    _requestMatcher = new RequestMatcher(matchingRule);
		    _log = (loggerFactory ?? HttpMockLogging.GetLoggerFactory()).CreateLogger<RequestProcessor>();
		}

		public void OnRequest(IHttpRequestHead request, Stream requestBody, Action<HttpMockResponseHead, byte[]> respond) {
			_log.LogDebug("Start Processing request for : {Method}:{Uri}", SanitizeForLog(request.Method), SanitizeForLog(request.Uri));
			if (GetHandlerCount() < 1) {
				using var noHandlersActivity = HttpMockActivitySource.Source.StartActivity("httpmock.request");
				noHandlersActivity?.SetTag("http.request.method", request.Method);
				noHandlersActivity?.SetTag("url.path", request.Uri);
				noHandlersActivity?.SetTag("httpmock.matched", false);
				noHandlersActivity?.SetTag("http.response.status_code", NotFoundStatusCode);
				ReturnHttpMockNotFound(respond);
				return;
			}

			string bufferedBody = null;
			if (request.HasEntityBody && requestBody != null)
			{
				try
				{
					using var reader = new StreamReader(requestBody, Encoding.UTF8);
					bufferedBody = reader.ReadToEnd();
				}
				catch (Exception error)
				{
					_log.LogDebug(error, "Error while reading body");
				}
			}

			var handler = _requestMatcher.Match(request, _handlers, bufferedBody);

			if (handler == null) {
				_log.LogDebug("No Handlers matched");
				using var noMatchActivity = HttpMockActivitySource.Source.StartActivity("httpmock.request");
				noMatchActivity?.SetTag("http.request.method", request.Method);
				noMatchActivity?.SetTag("url.path", request.Uri);
				noMatchActivity?.SetTag("httpmock.matched", false);
				noMatchActivity?.SetTag("http.response.status_code", NotFoundStatusCode);
				ReturnHttpMockNotFound(respond);
				return;
			}

			_ = HandleRequest(request, bufferedBody, respond, handler)
				.ContinueWith(
					t => _log.LogError(t.Exception?.InnerException ?? t.Exception, "Unhandled error processing request"),
					TaskContinuationOptions.OnlyOnFaulted);
		}

	    private async Task HandleRequest(IHttpRequestHead request, string bufferedBody, Action<HttpMockResponseHead, byte[]> respond, IRequestHandler handler)
	    {
	        using var activity = HttpMockActivitySource.Source.StartActivity("httpmock.request");
	        activity?.SetTag("http.request.method", request.Method);
	        activity?.SetTag("url.path", request.Uri);
	        activity?.SetTag("httpmock.matched", true);

	        if (_log.IsEnabled(LogLevel.Debug))
	        {
	            _log.LogDebug("Matched a handler {Method}:{Path} {QueryParams}", handler.Method, handler.Path, DumpQueryParams(handler.QueryParams));
	        }

            if (handler.ResponseDelay > TimeSpan.Zero)
            {
                activity?.AddEvent(new ActivityEvent("response.delay.start"));
                await Task.Delay(handler.ResponseDelay);
                activity?.AddEvent(new ActivityEvent("response.delay.end"));
            }

	        byte[] responseBody = request.Method != "HEAD"
	            ? handler.ResponseBuilder.BuildBody(request.Headers)
	            : null;

	        handler.RecordRequest(request, bufferedBody);
	        if (bufferedBody != null && _log.IsEnabled(LogLevel.Debug))
	        {
	            _log.LogDebug("Body: {Body}", SanitizeForLog(bufferedBody));
	        }

	        var responseHead = handler.ResponseBuilder.BuildHeaders(responseBody?.Length);
	        var statusParts = responseHead.Status?.Split(' ');
	        var statusCode = statusParts is { Length: > 0 } ? statusParts[0] : null;
	        activity?.SetTag("http.response.status_code", statusCode);
	        respond(responseHead, responseBody);
	        _log.LogDebug("End Processing request for : {Method}:{Uri}", SanitizeForLog(request.Method), SanitizeForLog(request.Uri));
	    }

		private static string SanitizeForLog(string value) =>
			value?.Replace("\r", "\\r").Replace("\n", "\\n");

		private int GetHandlerCount() {
			return _handlers.Count;
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
				Status = $"{404} NotFound",
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