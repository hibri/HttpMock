using System;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		RequestHandler Stub(Func<RequestProcessor, RequestHandler> func);
		IHttpServer WithNewContext();
		IHttpServer WithNewContext(string baseUri);
		void Start();
		void Dispose();
		string WhatDoIHave();
		bool IsAvailable();
	}
}