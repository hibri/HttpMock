using Kayak.Http;

namespace HttpMock
{
    public class ReceivedRequest
    {
        public HttpRequestHead RequestHead { get; private set; }
        public string Body { get; private set; }

        internal ReceivedRequest(HttpRequestHead head, string body)
        {
            RequestHead = head;
            Body = body;
        }
    }
}