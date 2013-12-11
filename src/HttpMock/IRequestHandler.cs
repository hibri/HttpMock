using System.Collections.Generic;

namespace HttpMock
{
    public interface IRequestHandler {
		string Path { get; set; }
		string Method { get; set; }
		RequestProcessor RequestProcessor { get; set; }
		IDictionary<string, string> QueryParams { get; set; }
		ResponseBuilder ResponseBuilder { get; }
		
	}
}