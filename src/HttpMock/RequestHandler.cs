﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace HttpMock
{
	public class RequestHandler : IRequestHandler, IRequestStub
	{
		private readonly ResponseBuilder _webResponseBuilder = new ResponseBuilder();
		private int _requestCount;
		private string _requestBody;
	    private readonly IList<Func<Uri, bool>> _constraints = new List<Func<Uri, bool>>();

	    public RequestHandler(string path, RequestProcessor requestProcessor) {
			Path = path;
			RequestProcessor = requestProcessor;
			QueryParams = new Dictionary<string, string>();
		}

		public string Path { get; set; }
		public string Method { get; set; }
		public RequestProcessor RequestProcessor { get; set; }
		public IDictionary<string, string> QueryParams { get; set; }

		public ResponseBuilder ResponseBuilder {
			get { return _webResponseBuilder; }
		}

		public IRequestStub Return(string responseBody) {
			_webResponseBuilder.Return(responseBody);
			return this;
		}

		public IRequestStub ReturnFile(string pathToFile) {
			_webResponseBuilder.WithFile(pathToFile);
			
			return this;
		}

		public IRequestStub ReturnFileRange(string pathToFile, int from, int to)
		{
			_webResponseBuilder.WithFileRange(pathToFile, from, to);
			
			return this;
		}

		public IRequestStub WithParams(IDictionary<string, string> nameValueCollection) {
			QueryParams = nameValueCollection;
			return this;
		}

		public void OK() {
			WithStatus(HttpStatusCode.OK);
		}

		public void WithStatus(HttpStatusCode httpStatusCode) {
			ResponseBuilder.WithStatus(httpStatusCode);
			RequestProcessor.Add(this);
		}

		public void NotFound() {
			WithStatus(HttpStatusCode.NotFound);
		}

		public RequestHandler AsXmlContent() {
			return AsContentType("text/xml");
		}

		public RequestHandler AsContentType(string contentType) {
			ResponseBuilder.WithContentType(contentType);
			return this;
		}

		public RequestHandler AddHeader(string header, string headerValue) {
			ResponseBuilder.AddHeader(header, headerValue);
			return this;
		}

        public RequestHandler WithUrlConstraint(Func<Uri, bool> constraint)
        {
            _constraints.Add(constraint);
            return this;
        }

		public override string ToString() {
			var sb = new StringBuilder();
			sb.AppendFormat("{0}:{1}{2}", Path, Method, Environment.NewLine);
			foreach (var param in QueryParams) {
				sb.AppendLine(string.Format("{0}:{1}", param.Key, param.Value));
			}
			return sb.ToString();
		}

		public int RequestCount() {
			return _requestCount;
		}

		public void RecordRequest() {
			_requestCount++;
		}

		public void AddBody(string body) {
			_requestBody = body;
		}

		public string GetBody() {
			return _requestBody;
		}


	    public bool CanVerifyConstraintsFor(Uri uriWithBlah)
	    {
	        return _constraints.All(c => c(uriWithBlah));
	    }
	}
}