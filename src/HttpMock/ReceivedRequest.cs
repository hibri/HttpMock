namespace HttpMock
{
    public class ReceivedRequest
    {
        public IHttpRequestHead RequestHead { get; private set; }
        public string Body { get; private set; }

        internal ReceivedRequest(IHttpRequestHead head, string body)
        {
            RequestHead = head;
            Body = body;
        }
    }
}