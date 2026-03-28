using System;
using System.IO;

namespace HttpMock
{
	public interface IRequestProcessor
	{
		void OnRequest(HttpRequestHead request, Stream requestBody, Action<HttpMockResponseHead, byte[]> respond);
		IRequestVerify FindHandler(string method, string path);
		void Add(RequestHandler requestHandler);
		void ClearHandlers();
		string WhatDoIHave();
	}
}