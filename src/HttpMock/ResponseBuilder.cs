using System.Collections.Generic;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public class ResponseBuilder

	{
		private string _body;


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
			       		Status = "200 OK",
			       		Headers = new Dictionary<string, string>
			       		          	{
			       		          		{"Content-Type", "text/plain"},
			       		          		{"Content-Length", GetBody().Length.ToString()},
			       		          	}
			       	};
		}

		private string GetBody() {
			return _body;
		}
	}
}