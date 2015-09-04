using Kayak.Http;

namespace HttpMock
{
    public interface IRequestVerify
    {
        int RequestCount();
        void RecordRequest(HttpRequestHead request, string body);
        string GetBody();
        ReceivedRequest LastRequest();
    }
}