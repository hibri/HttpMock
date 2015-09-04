using System.Collections.Generic;
using Kayak.Http;

namespace HttpMock
{
    public interface IRequestHandler {
		string Path { get; set; }
		string Method { get; set; }
		RequestProcessor RequestProcessor { get; set; }
		IDictionary<string, string> QueryParams { get; set; }
		ResponseBuilder ResponseBuilder { get; }
        bool CanVerifyConstraintsFor(string url);
        void RecordRequest(HttpRequestHead request, string body);
    }
}