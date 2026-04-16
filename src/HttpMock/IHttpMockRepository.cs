using System;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	public interface IHttpMockRepository 
	{
		IHttpServer At(Uri uri);
		IHttpServer At(string uri);
		IHttpServer At(Uri uri, ILoggerFactory loggerFactory);
		IHttpServer At(string uri, ILoggerFactory loggerFactory);
	}
}