using System;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		RequestHandler Stub(Func<RequestProcessor, RequestHandler> func);
		RequestHandler AssertWasCalled(Func<RequestVerifier, RequestHandler> func);
		IHttpServer WithNewContext();
		IHttpServer WithNewContext(string baseUri);
		void Start();
		string WhatDoIHave();
		bool IsAvailable();
	}
}