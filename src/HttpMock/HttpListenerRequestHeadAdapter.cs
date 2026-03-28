using System.Collections.Generic;
using System.Net;

namespace HttpMock
{
    internal class HttpListenerRequestHeadAdapter : IHttpRequestHead
    {
        private readonly HttpListenerRequest _request;

        public HttpListenerRequestHeadAdapter(HttpListenerRequest request)
        {
            _request = request;
        }

        public string Method => _request.HttpMethod;

        public string Uri => _request.Url.PathAndQuery;

        public IDictionary<string, string> Headers
        {
            get
            {
                var dict = new Dictionary<string, string>();
                foreach (string key in _request.Headers)
                {
                    if (key != null)
                        dict[key] = _request.Headers[key];
                }
                return dict;
            }
        }

        public bool HasBody() => _request.HasEntityBody;
    }
}
