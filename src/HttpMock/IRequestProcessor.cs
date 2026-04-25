using System;
using System.IO;
using System.Threading.Tasks;

namespace HttpMock
{
	public interface IRequestProcessor
	{
		Task OnRequest(IHttpRequestHead request, Stream requestBody, Action<HttpMockResponseHead, byte[]> respond);
		IRequestVerify FindHandler(string method, string path);
		void Add(RequestHandler requestHandler);
		void ClearHandlers();
		string WhatDoIHave();
	}
}