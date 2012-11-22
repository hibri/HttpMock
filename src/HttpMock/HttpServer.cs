using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public class HttpServer : IHttpServer
	{
		private readonly RequestProcessor _requestProcessor;
		private readonly RequestWasCalled _requestWasCalled;
		private readonly RequestWasNotCalled _requestWasNotCalled;
		private readonly IScheduler _scheduler;
		private readonly Uri _uri;
		private IDisposable _disposableServer;

		private Thread _thread;
		private readonly RequestHandlerFactory _requestHandlerFactory;

		public HttpServer(Uri uri) {
			_uri = uri;
			_scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
			_requestProcessor = new RequestProcessor(new EndpointMatchingRule(), new RequestHandlerList());
			_requestWasCalled = new RequestWasCalled(_requestProcessor);
			_requestWasNotCalled = new RequestWasNotCalled(_requestProcessor);
			_requestHandlerFactory = new RequestHandlerFactory(_requestProcessor);
		}

		public void Start() {
			_thread = new Thread(StartListening);
			_thread.Start();
			if (!IsAvailable()) {
				throw new InvalidOperationException("Kayak server not listening yet.");
			}
		}

		public bool IsAvailable() {
			const int timesToWait = 5;
			int attempts = 0;
			using (var tcpClient = new TcpClient()) {
				while (attempts < timesToWait) {
					try {
						tcpClient.Connect(_uri.Host, _uri.Port);
						return tcpClient.Connected;
					} catch (SocketException) {}

					Thread.Sleep(100);
					attempts++;
				}
				return false;
			}
		}

		public void Dispose() {
			_scheduler.Stop();
			_scheduler.Dispose();
			_disposableServer.Dispose();
		}

		public RequestHandler Stub(Func<RequestHandlerFactory, RequestHandler> func) {
			return func.Invoke(_requestHandlerFactory);
		}

		public RequestHandler AssertWasCalled(Func<RequestWasCalled, RequestHandler> func) {
			return func.Invoke(_requestWasCalled);
		}

		public RequestHandler AssertWasNotCalled(Func<RequestWasNotCalled, RequestHandler> func) {
			return func.Invoke(_requestWasNotCalled);
		}

		public IHttpServer WithNewContext() {
			_requestProcessor.ClearHandlers();
			return this;
		}

		public IHttpServer WithNewContext(string baseUri) {
			WithNewContext();
			return this;
		}

		public string WhatDoIHave() {
			return _requestProcessor.WhatDoIHave();
		}

		private void StartListening() {
			var ipEndPoint = new IPEndPoint(IPAddress.Any, _uri.Port);
			_scheduler.Post(() =>{
			                	_disposableServer = KayakServer.Factory
			                		.CreateHttp(_requestProcessor, _scheduler)
			                		.Listen(ipEndPoint);
			                });

			_scheduler.Start();
		}
	}
}