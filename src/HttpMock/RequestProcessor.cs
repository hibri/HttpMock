using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kayak;
using Kayak.Http;
using log4net;

namespace HttpMock
{
	public interface IRequestProcessor
	{
		RequestHandler FindHandler(string path, string method);
	}

	public class RequestProcessor : IHttpRequestDelegate, IRequestProcessor
	{
		private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly IMatchingRule _matchingRule;
		private List<RequestHandler> _handlers = new List<RequestHandler>();

		public RequestProcessor() {
			_matchingRule = new EndpointMatchingRule();
		}

		public RequestProcessor(IMatchingRule matchingRule) {
			_matchingRule = matchingRule;
		}

		public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response) {
			_log.DebugFormat("Start Processing request for : {0}:{1}", request.Method, request.Uri);
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
			_log.DebugFormat("Matched a handler {0},{1}, {2}", handler.Method, handler.Path, DumpQueryParams(handler.QueryParams));
			handler.RecordRequest();
			IDataProducer dataProducer = request.Method != "HEAD" ? handler.ResponseBuilder.BuildBody() : null;
			if (request.HasBody()) {
				body.Connect(new BufferedConsumer(
					bufferedBody =>
						{
						handler.AddBody(bufferedBody);
						response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
						},
					error =>
					{
						_log.DebugFormat("Error while reading body {0}", error.Message);
						response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
					}
					));
			} else {
				response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
			}
			_log.DebugFormat("End Processing request for : {0}:{1}", request.Method, request.Uri);
			return;
		}

		public RequestHandler FindHandler(string method, string path) {
			string cleanedPath = path;
			return _handlers.Where(x => x.Path == cleanedPath && x.Method == method).FirstOrDefault();
		}

		private static string DumpQueryParams(IDictionary<string, string> queryParams) {
			var sb = new StringBuilder();
			foreach (var param in queryParams) {
				sb.AppendFormat("{0}={1}&", param.Key, param.Value);
			}
			return sb.ToString();
		}

		private static void ReturnHttpMockNotFound(IHttpResponseDelegate response) {
			var dictionary = new Dictionary<string, string>
			{
				{HttpHeaderNames.ContentLength, "0"},
				{"SevenDigital-HttpMockError", "No handler found to handle request"}
			};

			var notFoundResponse = new HttpResponseHead
			{Status = string.Format("{0} {1}", 404, "NotFound"), Headers = dictionary};
			response.OnResponse(notFoundResponse, null);
		}

		public void ClearHandlers() {
			_handlers = new List<RequestHandler>();
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