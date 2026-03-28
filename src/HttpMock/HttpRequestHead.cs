using System.Collections.Generic;

namespace HttpMock
{
    public class HttpRequestHead : IHttpRequestHead
    {
        public string Method { get; set; }
        public string Uri { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public bool HasEntityBody { get; set; }
    }
}
