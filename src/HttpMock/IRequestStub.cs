using System;
using System.Collections.Generic;
using System.Net;

namespace HttpMock
{
	public interface IRequestStub
	{
		IRequestStub Return(string responseBody);
		IRequestStub Return(Func<string> responseBody);
		IRequestStub Return(byte[] responseBody);
		IRequestStub ReturnFile(string pathToFile);
		IRequestStub WithParams(IDictionary<string, string> nameValueCollection);
		IRequestStub WithHeaders(IDictionary<string, string> nameValueCollection);
		void OK();
		void WithStatus( HttpStatusCode httpStatusCode);
		void NotFound();
		IRequestStub AsXmlContent();
		IRequestStub AsContentType( string contentType);
		IRequestStub AddHeader( string header, string headerValue);
		IRequestStub WithUrlConstraint(Func<string, bool> constraint);
	    IRequestStub ReturnFileRange(string pathToFile, int from, int to);
        IRequestStub WithDelay(int milliseconds);
        IRequestStub WithDelay(TimeSpan timeSpan);
	}
}