using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace HttpMock
{
	public class HttpServer : IHttpServer
	{
		private readonly ILogger<HttpServer> _log;
		private readonly RequestHandlerFactory _requestHandlerFactory;
		private readonly IRequestProcessor _requestProcessor;
		private readonly Uri _uri;
		private HttpListener _listener;
		private volatile bool _running;

		/// <summary>
		/// Creates a new <see cref="HttpServer"/> bound to <paramref name="uri"/>.
		/// Pass your application's <see cref="ILoggerFactory"/> to enable logging via any
		/// Microsoft.Extensions.Logging-compatible provider (e.g. console, OpenTelemetry).
		/// When <paramref name="loggerFactory"/> is <see langword="null"/> the globally
		/// configured <see cref="HttpMockLogging"/> factory is used, falling back to a
		/// no-op logger.
		/// </summary>
		public HttpServer(Uri uri, ILoggerFactory loggerFactory = null)
		{
			_uri = uri;
			_log = (loggerFactory ?? HttpMockLogging.GetLoggerFactory()).CreateLogger<HttpServer>();
			_requestProcessor = new RequestProcessor(new EndpointMatchingRule(), new RequestHandlerList(), loggerFactory);
			_requestHandlerFactory = new RequestHandlerFactory(_requestProcessor);
		}

		private readonly object _startLock = new object();

		public void Start()
		{
			lock (_startLock)
			{
				if (_running) return;
				_listener = new HttpListener();

				var host = _uri.Host;
				if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && !RuntimeInformation.IsOSPlatform(OSPlatform.OSX)
				    && !string.IsNullOrEmpty(host) && host != "+" && host != "*" && host != "localhost")
				{
					_listener.Prefixes.Add($"http://{host}:{_uri.Port}/");
				}

				_listener.Prefixes.Add($"http://+:{_uri.Port}/");

				_listener.Start();
				_running = true;
				_ = ListenLoopAsync();
			}
			if (!IsAvailable())
			{
				throw new InvalidOperationException("HttpListener server not listening yet.");
			}
		}

		public bool IsAvailable()
		{
			const int timesToWait = 5;
			var attempts = 0;
			var connectHost = _uri.Host.TrimEnd('.');
			if (string.IsNullOrEmpty(connectHost))
				connectHost = "localhost";
			using (var tcpClient = new TcpClient())
			{
				while (attempts < timesToWait)
				{
					try
					{
						tcpClient.Connect(connectHost, _uri.Port);
						return tcpClient.Connected;
					}
					catch (SocketException)
					{
					}

					Thread.Sleep(100);
					attempts++;
				}
				return false;
			}
		}

		public IRequestProcessor GetRequestProcessor()
		{
			return _requestProcessor;
		}

		public void Dispose()
		{
			_running = false;
			if (_listener != null)
			{
				try { _listener.Stop(); } catch { }
				try { _listener.Close(); } catch { }
			}
		}

		public IRequestStub Stub(Func<RequestHandlerFactory, IRequestStub> func)
		{
			return func.Invoke(_requestHandlerFactory);
		}

		public IHttpServer WithNewContext()
		{
			_requestProcessor.ClearHandlers();
			return this;
		}

		public string WhatDoIHave()
		{
			return _requestProcessor.WhatDoIHave();
		}

		private async Task ListenLoopAsync()
		{
			while (_running)
			{
				try
				{
					var context = await _listener.GetContextAsync();
					_ = HandleContextAsync(context).ContinueWith(
						t => _log.LogError(t.Exception?.InnerException ?? t.Exception, "Unhandled error in request handler"),
						TaskContinuationOptions.OnlyOnFaulted);
				}
				catch (HttpListenerException)
				{
					if (!_running) break;
				}
				catch (ObjectDisposedException)
				{
					break;
				}
				catch (Exception ex)
				{
					_log.LogError(ex, "Error in listen loop");
				}
			}
		}

		private async Task HandleContextAsync(HttpListenerContext context)
		{
			try
			{
				var headers = new Dictionary<string, string>(context.Request.Headers.Count);
				foreach (string key in context.Request.Headers)
				{
					if (key != null)
						headers[key] = context.Request.Headers[key];
				}

				var requestHead = new HttpRequestHead
				{
					Method = context.Request.HttpMethod,
					Uri = context.Request.Url.PathAndQuery,
					Headers = headers,
					HasEntityBody = context.Request.HasEntityBody
				};
				Stream body = context.Request.HasEntityBody ? context.Request.InputStream : null;

				await _requestProcessor.OnRequest(requestHead, body, (responseHead, responseBody) =>
					WriteResponse(context.Response, responseHead, responseBody));
			}
			catch (Exception ex)
			{
				_log.LogError(ex, "Error handling request");
				try { context.Response.StatusCode = 500; context.Response.Close(); } catch { }
			}
		}

		private static void WriteResponse(HttpListenerResponse response, HttpMockResponseHead head, byte[] body)
		{
			try
			{
				var parts = head.Status.Split(new[] { ' ' }, 2);
				int statusCode;
				response.StatusCode = int.TryParse(parts[0], out statusCode) ? statusCode : 500;
				if (parts.Length > 1)
					response.StatusDescription = parts[1];

				if (head.Headers != null)
				{
					foreach (var header in head.Headers)
					{
						if (string.Equals(header.Key, "Content-Type", StringComparison.OrdinalIgnoreCase))
						{
							response.ContentType = header.Value;
						}
						else if (!string.Equals(header.Key, "Content-Length", StringComparison.OrdinalIgnoreCase))
						{
							response.Headers[header.Key] = header.Value;
						}
					}
				}

				if (body != null && body.Length > 0)
				{
					response.ContentLength64 = body.Length;
					response.OutputStream.Write(body, 0, body.Length);
				}
				else
				{
					response.ContentLength64 = 0;
				}
			}
			finally
			{
				try { response.Close(); } catch { }
			}
		}
	}
}