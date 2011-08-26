using System.Collections.Generic;
using System.Net;

namespace HttpMock
{
	public interface IRequestStub
	{
		IRequestStub Return(string responseBody);
		IRequestStub ReturnFile(string pathToFile);
		IRequestStub WithParams(IDictionary<string, string> nameValueCollection);
		void OK();
		void WithStatus( HttpStatusCode httpStatusCode);
		void NotFound();
		RequestHandler AsXmlContent();
		RequestHandler AsContentType( string contentType);
		RequestHandler AddHeader( string header, string headerValue);
	}
}