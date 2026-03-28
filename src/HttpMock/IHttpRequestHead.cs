using System.Collections.Generic;

namespace HttpMock
{
    public interface IHttpRequestHead
    {
        string Method { get; }
        string Uri { get; }
        IDictionary<string, string> Headers { get; }
        bool HasEntityBody { get; }
    }
}
