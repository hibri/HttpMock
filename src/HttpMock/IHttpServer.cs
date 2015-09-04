using System;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		IRequestStub Stub(Func<RequestHandlerFactory, IRequestStub> func);
		IRequestVerify AssertWasCalled(Func<RequestWasCalled, IRequestVerify> func);
		IHttpServer WithNewContext();
		IHttpServer WithNewContext(string baseUri);
		void Start();
		string WhatDoIHave();
		bool IsAvailable();
		IRequestVerify AssertWasNotCalled(Func<RequestWasNotCalled, IRequestVerify> func);
	}
}