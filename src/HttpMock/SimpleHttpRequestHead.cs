using System;
using System.Collections.Generic;

namespace HttpMock
{
    public class SimpleHttpRequestHead : IHttpRequestHead
    {
        public string Method { get; set; }
        public string Uri { get; set; }
        public IDictionary<string, string> Headers { get; set; }

        public bool HasBody()
        {
            if (Headers == null) return false;
            foreach (var key in Headers.Keys)
            {
                if (key.Equals("Content-Length", StringComparison.OrdinalIgnoreCase)
                    && Headers[key] != "0")
                    return true;
                if (key.Equals("Transfer-Encoding", StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
