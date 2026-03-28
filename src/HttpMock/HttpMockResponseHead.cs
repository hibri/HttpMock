using System.Collections.Generic;

namespace HttpMock
{
    public class HttpMockResponseHead
    {
        public string Status { get; set; }
        public IDictionary<string, string> Headers { get; set; }
    }
}
