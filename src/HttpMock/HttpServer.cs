using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		RequestHandler Stub(Func<RequestProcessor, RequestHandler> func);
		IStubHttp WithNewContext();
		IStubHttp WithNewContext(string baseUri);
		void Start();
	}

	public class HttpServer : IHttpServer, IStubHttp
	{
		protected RequestProcessor _requestProcessor;
		protected IScheduler _scheduler;
		private Thread _thread;
		private Uri _uri;
		private IDisposable _disposableServer;

		public HttpServer(Uri uri)
		{
			_uri = uri;
			_scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
			_requestProcessor = new RequestProcessor();
		}

		public void Start() {

			_thread = new Thread(StartListening);
			_thread.Start();
			WaitTillServerIsListening();
		}

		private void WaitTillServerIsListening() {
			const int timesToWait = 5;
			using(var tcpClient = new TcpClient() ) {
				int attempts = 0;
				while (attempts < timesToWait) {
					TryConnect(tcpClient);
					if (tcpClient.Connected) {
						break;
					}
					Thread.Sleep(10);
					attempts++;
				}
			}
		}

		private void TryConnect(TcpClient tcpClient) {
			tcpClient.Connect(_uri.Host, _uri.Port);
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
			try
			{
				_disposableServer.Dispose();
				_thread.Abort();
			}
			catch (ThreadAbortException) {
			}
			
			_scheduler.Stop();
			_scheduler.Dispose();
		}

		public RequestHandler Stub(Func<RequestProcessor, RequestHandler> func)
		{
			return func.Invoke(_requestProcessor);
		}

		public IStubHttp WithNewContext() {
			_requestProcessor.ClearHandlers();
			return this;
		}

		public IStubHttp WithNewContext(string baseUri) {
			_requestProcessor.SetBaseUri(baseUri);
			WithNewContext();
			return this;
		}
	}
}