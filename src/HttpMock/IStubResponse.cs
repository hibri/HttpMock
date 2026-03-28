namespace HttpMock
{
	public interface IStubResponse
	{
		ResponseBuilder Get(IHttpRequestHead request);
	}
}