using System;

namespace HttpMock
{
	public interface IHttpServerBuilder
	{
		IHttpServer Build(Uri uri);
	}
}