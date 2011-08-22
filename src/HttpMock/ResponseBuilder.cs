using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public class ResponseBuilder
	{
		private  HttpStatusCode _httpStatusCode = HttpStatusCode.OK;
		private string _contentType = "text/plain";
		private IDataProducer _responseBodyBuilder = new BufferedBody("");
		private int _contentLength = 0;
		private Dictionary<string, string> _headers = new Dictionary<string, string>();

		public ResponseBuilder Return(string body) {
			_responseBodyBuilder = new BufferedBody(body);
			_contentLength = body.Length;
			return this;
		}

		public IDataProducer BuildBody() {
			return _responseBodyBuilder;
		}

		public HttpResponseHead BuildHeaders() {
			AddHeader(HttpHeaderNames.ContentType, _contentType); 
			AddHeader(HttpHeaderNames.ContentLength, _contentLength.ToString());

			var headers = new HttpResponseHead
			{
				Status = string.Format("{0} {1}", (int)_httpStatusCode, _httpStatusCode),
				Headers = _headers
			};
			return headers;

			
		}

		public void WithStatus(HttpStatusCode httpStatusCode) {
			_httpStatusCode = httpStatusCode;
		}

		public void WithContentType(string contentType) {
			_contentType = contentType;
		}

		public void WithFile(string pathToFile) {
			if(File.Exists(pathToFile)) {
				var fileInfo = new FileInfo(pathToFile);
				_contentLength = (int)fileInfo.Length;
				_responseBodyBuilder = new FileResponseBody(pathToFile);
			} else {
				throw new InvalidOperationException("File does not exist/accessible at :" + pathToFile);
			}
		}

		public void AddHeader(string header, string headerValue) {
			if (_headers.ContainsKey(header))
			{
				_headers[header] = headerValue;
			}
			else {
				_headers.Add(header, headerValue);
			}
		}
	}
}