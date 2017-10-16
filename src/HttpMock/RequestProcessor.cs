using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Kayak;
using Kayak.Http;
using System.Collections.Concurrent;

namespace HttpMock
{
    public class RequestProcessor : IRequestProcessor
    {
        private static readonly ILog _log = LogFactory.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ConcurrentDictionary<Guid, RequestHandlerList> _handlers { get; set; }
        private readonly RequestMatcher _requestMatcher;

        public RequestProcessor(IMatchingRule matchingRule, ConcurrentDictionary<Guid, RequestHandlerList> requestHandlers) {
            _handlers = requestHandlers;
            _requestMatcher = new RequestMatcher(matchingRule);
        }

        public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response) {
            _log.DebugFormat("Start Processing request for : {0}:{1}", request.Method, request.Uri);
            if (GetHandlerCount() < 1) {
                ReturnHttpMockNotFound(response);
                return;
            }
            IRequestHandler handler = null;
            Guid currentRequestId = Guid.Empty;
            if (request.Headers == null)
            {
                _log.DebugFormat("No header specified");
            }
            else if (request.Headers.ContainsKey(Constants.MockSessionHeaderKey))
            {
                currentRequestId = new Guid(request.Headers[Constants.MockSessionHeaderKey]);
            }
            else if (request.Headers.ContainsKey(Constants.CookieHeaderKey))
            {
                var mockSessionId = request.Headers[Constants.CookieHeaderKey].Split("=".ToCharArray());
                currentRequestId = new Guid(mockSessionId[1]);
            }
            RequestHandlerList requestHandler;
            if (_handlers.TryGetValue(currentRequestId, out requestHandler))
            {
                lock (requestHandler)
                {

                    handler = _requestMatcher.Match(request, requestHandler);
                }
            }
            if (handler == null)
            {
                _log.DebugFormat("No Handlers matched");
                ReturnHttpMockNotFound(response);
                return;
            }
            HandleRequest(request, body, response, handler);
        }

        private static void HandleRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response, IRequestHandler handler) {
            _log.DebugFormat("Matched a handler {0}:{1} {2}", handler.Method, handler.Path, DumpQueryParams(handler.QueryParams));
            IDataProducer dataProducer = GetDataProducer(request, handler);
            if (request.HasBody())
            {
                body.Connect(new BufferedConsumer(
                    bufferedBody =>
                    {
                        handler.RecordRequest(request, bufferedBody);
                        _log.DebugFormat("Body: {0}", bufferedBody);
                        response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
                    },
                    error =>
                    {
                        _log.DebugFormat("Error while reading body {0}", error.Message);
                        response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
                    }
                    ));
            }
            else
            {
                response.OnResponse(handler.ResponseBuilder.BuildHeaders(), dataProducer);
                handler.RecordRequest(request, null);
            }
            _log.DebugFormat("End Processing request for : {0}:{1}", request.Method, request.Uri);
        }

        private static IDataProducer GetDataProducer(HttpRequestHead request, IRequestHandler handler) {
            return request.Method != "HEAD" ? handler.ResponseBuilder.BuildBody(request.Headers) : null;
        }

        private int GetHandlerCount() {
            return _handlers.Count();
        }

        public IRequestVerify FindHandler(string method, string path, Guid sessionId = default(Guid)) {
            RequestHandlerList finder;
            if (_handlers.TryGetValue(sessionId, out finder))
            {
                return (IRequestVerify)finder.FirstOrDefault(x => x.Path == path && x.Method == method);
            }
            return null;
        }

        private static string DumpQueryParams(IDictionary<string, string> queryParams) {
            var sb = new StringBuilder();
            foreach (var param in queryParams)
            {
                sb.AppendFormat("{0}={1}&", param.Key, param.Value);
            }
            return sb.ToString();
        }

        private static void ReturnHttpMockNotFound(IHttpResponseDelegate response) {
            var dictionary = new Dictionary<string, string>
            {
                {HttpHeaderNames.ContentLength, "0"},
                {"X-HttpMockError", "No handler found to handle request"}
            };

            var notFoundResponse = new HttpResponseHead
            { Status = string.Format("{0} {1}", 404, "NotFound"), Headers = dictionary };
            response.OnResponse(notFoundResponse, null);
        }

        public void ClearHandlers(Guid sessionId) {
            RequestHandlerList lst;
            _handlers.TryRemove(sessionId, out lst);
        }

        public void Add(RequestHandler requestHandler) {
            Guid currentRequestId = Guid.Empty;
            if (requestHandler.RequestHeaders.ContainsKey(Constants.MockSessionHeaderKey))
            {
                currentRequestId = new Guid(requestHandler.RequestHeaders[Constants.MockSessionHeaderKey]);
            }
            else if (requestHandler.RequestHeaders.ContainsKey(Constants.CookieHeaderKey))
            {
                var mockSessionId = requestHandler.RequestHeaders[Constants.CookieHeaderKey].Split("=".ToCharArray());
                currentRequestId = new Guid(mockSessionId[1]);
            }
            RequestHandlerList lst;
            if (_handlers.TryGetValue(currentRequestId, out lst))
            {
                lock (lst)
                {
                    lst.Add(requestHandler);
                    _handlers.TryUpdate(currentRequestId, lst, _handlers[currentRequestId]);
                }
            }
            else
            {
                lst = new RequestHandlerList { requestHandler };
                _handlers.TryAdd(currentRequestId, lst);
            }
        }

        public string WhatDoIHave() {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Handlers:");
            foreach (var requestHandler in _handlers)
            {
                foreach (RequestHandler handler in requestHandler.Value)
                {
                    stringBuilder.Append(handler.ToString());
                }
            }
            return stringBuilder.ToString();
        }
    }
}