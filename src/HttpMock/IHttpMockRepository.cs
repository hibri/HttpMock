using System;

namespace HttpMock
{
	public  interface IHttpMockRepository 
	{
		IHttpServer At(Uri uri);
		
		IHttpServer At(string uri);
	}
}