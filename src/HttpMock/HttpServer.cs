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
		private readonly IScheduler _scheduler;
		private readonly Uri _uri;
		private IDisposable _disposableServer;
		
		private Thread _thread;
		private readonly RequestVerifier _requestVerifier;

		public HttpServer(Uri uri)
		{
			_uri = uri;
			_scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
			_requestProcessor = new RequestProcessor();
			_requestVerifier = new RequestVerifier(_requestProcessor);
		}

		public void Start() {
			_thread = new Thread(StartListening);
			_thread.Start();
			if (!IsAvailable()) throw new InvalidOperationException("Kayak server not listening yet.");
		}

		public bool IsAvailable()
		{
			const int timesToWait = 5;
			int attempts = 0;
			using(var tcpClient = new TcpClient() ) {
				while (attempts < timesToWait) {
					tcpClient.Connect(_uri.Host, _uri.Port);
					if (tcpClient.Connected) {
						return true;
					}
					Thread.Sleep(100);
					attempts++;
				}
				return false;
			}
		}

		private void StartListening() {
			var ipEndPoint = new IPEndPoint(IPAddress.Any, _uri.Port);
			_scheduler.Post(() => {
				_disposableServer = KayakServer.Factory
					.CreateHttp(_requestProcessor, _scheduler)
					.Listen(ipEndPoint);
			});

			_scheduler.Start();
		}

		public void Dispose() {
			_scheduler.Stop();
			_scheduler.Dispose();
			_disposableServer.Dispose();
		}

		public RequestHandler Stub(Func<RequestProcessor, RequestHandler> func)
		{
			return func.Invoke(_requestProcessor);
		}

		public RequestHandler AssertWasCalled(Func<RequestVerifier, RequestHandler> func) {
			return func.Invoke(_requestVerifier);
		}

		public IHttpServer WithNewContext() {
			_requestProcessor.ClearHandlers();
			return this;
		}

		public IHttpServer WithNewContext(string baseUri) {
			_requestProcessor.SetBaseUri(baseUri);
			WithNewContext();
			return this;
		}

		public string WhatDoIHave()
		{
			return _requestProcessor.WhatDoIHave();
		}
	}
}