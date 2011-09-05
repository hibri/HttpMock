using System;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		RequestHandler Stub(Func<RequestHandlerFactory, RequestHandler> func);
		RequestHandler AssertWasCalled(Func<RequestWasCalled, RequestHandler> func);
		IHttpServer WithNewContext();
		IHttpServer WithNewContext(string baseUri);
		void Start();
		string WhatDoIHave();
		bool IsAvailable();
		RequestHandler AssertWasNotCalled(Func<RequestWasNotCalled, RequestHandler> func);
	}
}