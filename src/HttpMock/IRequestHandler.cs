using System;
using System.Collections.Generic;
using Kayak.Http;

namespace HttpMock
{
    public interface IRequestHandler {
		string Path { get; set; }
		string Method { get; set; }
        TimeSpan ResponseDelay { get; set; }
		IRequestProcessor RequestProcessor { get; set; }
		IDictionary<string, string> QueryParams { get; set; }
		IDictionary<string, string> RequestHeaders { get; set; }
		ResponseBuilder ResponseBuilder { get; }
        bool CanVerifyConstraintsFor(string url);
        void RecordRequest(HttpRequestHead request, string body);
    }
}