using System;
using System.Net;
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
		private ISchedulerDelegate _schedulerDelegate;
		protected KayakScheduler _scheduler;
		private Thread _thread;
		private Uri _uri;

		public HttpServer(Uri uri)
		{
			_uri = uri;
			_schedulerDelegate = new SchedulerDelegate();
			_scheduler = new KayakScheduler(_schedulerDelegate);
			_requestProcessor = new RequestProcessor();
			

			
		}

		public KayakScheduler Scheduler {
			get { return _scheduler; }
		}

		public void Start() {
			IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, _uri.Port);
			_scheduler.Post(() => 
			                Begin(ipEndPoint));

			_thread = new Thread(_scheduler.Start);
			_thread.Start();
		}

		private IDisposable Begin(IPEndPoint ipEndPoint) {
			IDisposable disposable = KayakServer.Factory
				.CreateHttp(_requestProcessor)
				.Listen(ipEndPoint);
			_requestProcessor.SetCloseObject(disposable);
			return disposable;
		}

		public void Dispose() {
			try
			{
				_requestProcessor.Stop();
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