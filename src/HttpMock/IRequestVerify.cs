using System.Collections.Generic;

namespace HttpMock
{
    public interface IRequestVerify
    {
        int RequestCount();
        void RecordRequest(IHttpRequestHead request, string body);
        string GetBody();
        ReceivedRequest LastRequest();
        IEnumerable<ReceivedRequest> GetObservedRequests();
    }
}