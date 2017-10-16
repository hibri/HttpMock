using Kayak.Http;
using System;

namespace HttpMock
{
    public interface IRequestProcessor : IHttpRequestDelegate
    {
        IRequestVerify FindHandler(string method, string path, Guid sessionId=default(Guid));
        void Add(RequestHandler requestHandler);
        void ClearHandlers(Guid sessionId);
        string WhatDoIHave();
    }
}