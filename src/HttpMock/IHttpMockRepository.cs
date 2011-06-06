using System;

namespace HttpMock
{
	public  interface IHttpMockRepository 
	{
		HttpServer At(Uri uri);
		
		HttpServer At(string uri);
	}
}