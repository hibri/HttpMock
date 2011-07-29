using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Kayak;
using Kayak.Http;

namespace HttpMock
{
	public interface IHttpServer : IDisposable
	{
		RequestHandler Stub(Func<RequestProcessor, RequestHandler> func);
		IStubHttp WithNewContext();
	}

	public class HttpServer : IHttpServer, IStubHttp
	{
		protected RequestProcessor _requestProcessor;
		private ISchedulerDelegate _schedulerDelegate;
		private Thread _thread;
		private Uri _uri;

		public HttpServer(Uri uri)
		{
			_uri = uri;
			_schedulerDelegate = new SchedulerDelegate();
			_requestProcessor = new RequestProcessor();
		}

		public void Start() {
			var ipEndPoint = new IPEndPoint(IPAddress.Any, _uri.Port);
			var scheduler = KayakScheduler.Factory.Create(new SchedulerDelegate());
			scheduler.Post(() => 
						KayakServer.Factory
							.CreateHttp(_requestProcessor, scheduler)
							.Listen(ipEndPoint)
			);

			Task.Factory.StartNew(() => scheduler.Start());
		}

		public void Dispose() {
			try {
				_requestProcessor.Stop();
				_thread.Abort();
			} catch (ThreadAbortException) {}

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