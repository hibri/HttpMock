using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public class RequestProcessor : IHttpRequestDelegate
	{
		private readonly string _applicationPath;
		private Dictionary<string, RequestHandler> _handlers;

		public RequestProcessor(Uri appBaseUri) {
			_applicationPath = appBaseUri.AbsolutePath;
		}

		public void OnRequest(HttpRequestHead request, IDataProducer body, IHttpResponseDelegate response) {

			string pathToMatch = request.Uri.Remove(request.Uri.IndexOf(_applicationPath, 0), _applicationPath.Length);
			RequestHandler handler = _handlers.Where(x => x.Key.Equals(pathToMatch)).FirstOrDefault().Value;
			if (handler != null) {
				var headers = handler.ResponseBuilder.BuildHeaders();

				var responseBody = handler.ResponseBuilder.BuildBody();
				response.OnResponse(headers, responseBody);
			}
			else {
				HttpResponseHead headers;
				var responseBody = GetNotStubbedResponse(request, out headers);
				response.OnResponse(headers, responseBody);
			}
		}

		public RequestHandler Get(string path) {
			var requestHandler = new RequestHandler(path, this);

			return requestHandler;
		}

		public void ClearHandlers() {
			_handlers = new Dictionary<string, RequestHandler>();
		}

		public void Add(RequestHandler requestHandler, HttpStatusCode httpStatusCode) {
			requestHandler.ResponseBuilder.WithStatus(httpStatusCode);
			_handlers.Add(requestHandler.Path, requestHandler);
		}


		private IDataProducer GetNotStubbedResponse(HttpRequestHead request, out HttpResponseHead headers) {
			IDataProducer body;
			string data = string.Format("{0} : Not Stubbed", request.Uri);
			headers = new HttpResponseHead
			          	{
			          		Status = string.Format("404 {0} Not Stubbed", request.Uri),
			          		Headers = new Dictionary<string, string>
			          		          	{
			          		          		{"Content-Type", "text/plain"},
			          		          		{"Content-Length", data.Length.ToString()},
			          		          	}
			          	};

			body = new BufferedBody(data);
			return body;
		}
	}
}