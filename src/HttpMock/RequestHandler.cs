using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace HttpMock
{
	public interface IRequestHandler {
		string Path { get; set; }
		string Method { get; set; }
		RequestProcessor RequestProcessor { get; set; }
		IDictionary<string, string> QueryParams { get; set; }
		ResponseBuilder ResponseBuilder { get; }
		
	}

	public class RequestHandler : IRequestHandler, IRequestStub
	{
		private readonly ResponseBuilder _webResponseBuilder = new ResponseBuilder();
		private int _requestCount;

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

		public IRequestStub Return(string responseBody)
		{

			_webResponseBuilder.Return(responseBody);
			return this;
		}

		public IRequestStub ReturnFile(string pathToFile)
		{
			var fileName = System.IO.Path.GetFileName(pathToFile);
			_webResponseBuilder.WithFile(pathToFile);
			_webResponseBuilder.AddHeader("Content-Disposition", string.Format("attachment; filename=\"{0}\"", fileName));
			return this;
		}

		public IRequestStub WithParams(IDictionary<string, string> nameValueCollection)
		{
			QueryParams = nameValueCollection;
			return this;
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendFormat("{0}:{1}{2}", Path, Method, Environment.NewLine);
			foreach(var param in QueryParams)
				sb.AppendLine(string.Format("{0}:{1}", param.Key, param.Value));
			return sb.ToString();
		}

		public int RequestCount() {
			return _requestCount;
		}

		public void RecordRequest() {
			_requestCount++;
		}



		public void OK()
		{
			WithStatus(HttpStatusCode.OK);
		}

		public void WithStatus( HttpStatusCode httpStatusCode)
		{
			ResponseBuilder.WithStatus(httpStatusCode);
			RequestProcessor.Add(this);
		}

		public  void NotFound()
		{
			WithStatus(HttpStatusCode.NotFound);
		}

		public RequestHandler AsXmlContent()
		{
			return AsContentType("text/xml");
		}

		public  RequestHandler AsContentType( string contentType)
		{
			ResponseBuilder.WithContentType(contentType);
			return this;
		}

		public RequestHandler AddHeader( string header, string headerValue)
		{
			ResponseBuilder.AddHeader(header, headerValue);
			return this;
		}
	}

	public interface IRequestStub
	{
		IRequestStub Return(string responseBody);
		IRequestStub ReturnFile(string pathToFile);
		IRequestStub WithParams(IDictionary<string, string> nameValueCollection);
		void OK();
		void WithStatus( HttpStatusCode httpStatusCode);
		void NotFound();
		RequestHandler AsXmlContent();
		RequestHandler AsContentType( string contentType);
		RequestHandler AddHeader( string header, string headerValue);
	}
}