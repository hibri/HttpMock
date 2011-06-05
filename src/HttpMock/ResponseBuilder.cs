using System.Collections.Generic;
using System.Net;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public class ResponseBuilder

	{
		private string _body;
		private  HttpStatusCode _httpStatusCode = HttpStatusCode.OK;


		public ResponseBuilder WithBody(string body) {
			_body = body;
			return this;
		}

		public IDataProducer BuildBody() {
			return new BufferedBody(GetBody());
		}

		public HttpResponseHead BuildHeaders() {
			return new HttpResponseHead
			       	{
			       		Status = string.Format("{0} {1}", (int)_httpStatusCode, _httpStatusCode),
			       		Headers = new Dictionary<string, string>
			       		          	{
			       		          		{HttpHeaderNames.ContentType, "text/plain"},
			       		          		{HttpHeaderNames.ContentLength, GetBody().Length.ToString()},
			       		          	}
			       	};
		}

		private string GetBody() {
			return _body;
		}

		public void WithStatus(HttpStatusCode httpStatusCode) {
			_httpStatusCode = httpStatusCode;
		}
	}
}