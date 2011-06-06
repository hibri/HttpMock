using System;

namespace HttpMock
{
	public interface IStubHttp
	{
		RequestHandler Stub(Func<RequestProcessor, RequestHandler> func);
		IStubHttp WithNewContext();
		IStubHttp WithNewContext(string baseUri);
	}
}