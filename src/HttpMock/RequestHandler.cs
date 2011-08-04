using System;
using System.Collections.Generic;
using System.Text;

namespace HttpMock
{
	public interface IRequestHandler {
		string Path { get; set; }
		string Method { get; set; }
		RequestProcessor RequestProcessor { get; set; }
		IDictionary<string, string> QueryParams { get; set; }
		ResponseBuilder ResponseBuilder { get; }
		RequestHandler Return( string responseBody);
		RequestHandler ReturnFile(string pathToFile);
		RequestHandler WithParams(IDictionary<string,string> nameValueCollection);
	}

	public class RequestHandler : IRequestHandler
	{
		private readonly ResponseBuilder _webResponseBuilder = new ResponseBuilder();

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

		public RequestHandler Return( string responseBody) {

			_webResponseBuilder.Return(responseBody);
			return this;
		}

		public RequestHandler ReturnFile(string pathToFile) {
			var fileName = System.IO.Path.GetFileName(pathToFile);
			_webResponseBuilder.WithFile(pathToFile);
			_webResponseBuilder.AddHeader("Content-Disposition", string.Format("attachment; filename=\"{0}\"", fileName));
			return this;
		}

		public RequestHandler WithParams(IDictionary<string,string> nameValueCollection) {
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
	}
}