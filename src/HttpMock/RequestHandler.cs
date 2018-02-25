using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Kayak.Http;

namespace HttpMock
{
	public class RequestHandler : IRequestHandler, IRequestStub, IRequestVerify
	{
		private readonly ResponseBuilder _webResponseBuilder = new ResponseBuilder();
		private readonly IList<Func<string, bool>> _constraints = new List<Func<string, bool>>();
		private readonly Queue<ReceivedRequest> _observedRequests = new Queue<ReceivedRequest>();

		public RequestHandler(string path, IRequestProcessor requestProcessor) {
			Path = path;
			RequestProcessor = requestProcessor;
			QueryParams = new Dictionary<string, string>();
			RequestHeaders = new Dictionary<string, string>();
		}

		public string Path { get; set; }
		public string Method { get; set; }
        public TimeSpan ResponseDelay { get; set; }
		public IRequestProcessor RequestProcessor { get; set; }
		public IDictionary<string, string> QueryParams { get; set; }
		public IDictionary<string, string> RequestHeaders { get; set; }

		public ResponseBuilder ResponseBuilder {
			get { return _webResponseBuilder; }
		}

		public IRequestStub Return(string responseBody) {
			_webResponseBuilder.Return(responseBody);
			return this;
		}

		public IRequestStub Return(Func<string> responseBody) {
			_webResponseBuilder.Return(responseBody);
			return this;
		}

		public IRequestStub Return(byte[] responseBody) {
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

		public IRequestStub WithHeaders(IDictionary<string, string> nameValueCollection)
		{
			RequestHeaders = nameValueCollection;
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

		public IRequestStub AsXmlContent() {
			return AsContentType("text/xml");
		}

		public IRequestStub AsContentType(string contentType) {
			ResponseBuilder.WithContentType(contentType);
			return this;
		}

		public IRequestStub AddHeader(string header, string headerValue) {
			ResponseBuilder.AddHeader(header, headerValue);
			return this;
		}

		public IRequestStub WithUrlConstraint(Func<string, bool> constraint)
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
			return _observedRequests.Count;
		}

		public void RecordRequest(HttpRequestHead request, string body)
		{
			_observedRequests.Enqueue(new ReceivedRequest(request, body));
		}

		public string GetBody() {
			return _observedRequests.Peek().Body;
		}

		public bool CanVerifyConstraintsFor(string url)
		{
			return _constraints.All(c => c(url));
		}

		public ReceivedRequest LastRequest()
		{
			return _observedRequests.Peek();
		}
		
		public IEnumerable<ReceivedRequest> GetObservedRequests()
        {
            return _observedRequests;
        }

        public IRequestStub WithDelay(int milliseconds)
        {
            ResponseDelay = ResponseDelay.Add(TimeSpan.FromMilliseconds(milliseconds));
            return this;
        }

        public IRequestStub WithDelay(TimeSpan timeSpan)
        {
            ResponseDelay = ResponseDelay.Add(timeSpan);
            return this;
        }
    }
}