using System;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		IRequestStub Stub(Func<RequestHandlerFactory, IRequestStub> func);
		IHttpServer WithNewContext();
		void Start();
		string WhatDoIHave();
		bool IsAvailable();
		IRequestProcessor GetRequestProcessor();
	}
}