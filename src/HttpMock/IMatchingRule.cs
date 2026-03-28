namespace HttpMock
{
    public interface IMatchingRule
	{
		bool IsEndpointMatch(IRequestHandler requestHandler, IHttpRequestHead request);
	}
}