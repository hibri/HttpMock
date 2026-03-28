namespace HttpMock
{
	public interface IStubResponse
	{
		ResponseBuilder Get(HttpRequestHead request);
	}
}