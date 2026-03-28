using System.Collections.Generic;
using Kayak.Http;

namespace HttpMock
{
    public class KayakHttpRequestHeadAdapter : IHttpRequestHead
    {
        private readonly HttpRequestHead _inner;

        public KayakHttpRequestHeadAdapter(HttpRequestHead inner)
        {
            _inner = inner;
        }

        public string Method => _inner.Method;
        public string Uri => _inner.Uri;
        public IDictionary<string, string> Headers => _inner.Headers;
        public bool HasBody() => _inner.HasBody();
    }
}
