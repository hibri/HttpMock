using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpEndPointTests
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private IHttpServer _stubHttp;
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		}

		[Test]
		public async Task SUT_should_return_stubbed_response()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			const string expected = "<xml><>response>Hello World</response></xml>";
			_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

			string result = await _httpClient.GetStringAsync($"{_hostUrl}/endpoint");

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public async Task SUT_should_return_stubbed_byte_array_response()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			byte[] expected =
				Encoding.UTF8.GetBytes(
					"<xml><>response>Change to bytes to simulate possible stream response</response></xml>");
			_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

			byte[] result = await _httpClient.GetByteArrayAsync($"{_hostUrl}/endpoint");

			Assert.That(result, Is.EqualTo(expected));
		}

		[TestCase(1)]
		[TestCase(100)]
		[TestCase(5000)]
		public async Task SUT_should_get_back_exact_content_in_the_last_request_body(int count)
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			string expected = string.Format("<xml><>response>{0}</response></xml>",
				string.Join(" ", Enumerable.Range(0, count)));
			var requestHandler = _stubHttp.Stub(x => x.Post("/endpoint"));

			requestHandler.Return(expected).OK();

			var content = new StringContent(expected, Encoding.UTF8, "application/xml");
			await _httpClient.PostAsync($"{_hostUrl}/endpoint", content);

			var requestBody = ((RequestHandler) requestHandler).LastRequest().Body;

			Assert.That(requestBody, Is.EqualTo(expected));
		}

		[Test]
		public async Task Should_get_the_last_request_that_was_sent()
		{
			var expected = "expectedbody";
			_stubHttp = HttpMockRepository.At(_hostUrl);
			var requestHandler = _stubHttp.Stub(x => x.Post("/endpoint"));
			requestHandler.Return(expected).OK();

			await _httpClient.PostAsync($"{_hostUrl}/endpoint",
				new StringContent("first", Encoding.UTF8, "application/xml"));
			await _httpClient.PostAsync($"{_hostUrl}/endpoint",
				new StringContent("second", Encoding.UTF8, "application/xml"));
			await _httpClient.PostAsync($"{_hostUrl}/endpoint",
				new StringContent(expected, Encoding.UTF8, "application/xml"));

			var requestBody = ((RequestHandler) requestHandler).LastRequest().Body;

			Assert.That(requestBody, Is.EqualTo(expected));
		}


		[Test]
		public async Task Should_start_listening_before_stubs_have_been_set()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return("listening")
				.OK();

			using (var tcpClient = new TcpClient())
			{
				var uri = new Uri(_hostUrl);

				tcpClient.Connect(uri.Host, uri.Port);

				Assert.That(tcpClient.Connected, Is.True);

				tcpClient.Close();
			}

			string result = await _httpClient.GetStringAsync($"{_hostUrl}/endpoint");

			Assert.That(result, Is.EqualTo("listening"));
		}

		[Test]
		public async Task Should_return_expected_ok_response()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			_stubHttp
				.Stub(x => x.Get("/api2/status"))
				.Return("Hello")
				.OK();

			_stubHttp
				.Stub(x => x.Get("/api2/echo"))
				.Return("Echo")
				.NotFound();

			_stubHttp
				.Stub(x => x.Get("/api2/echo2"))
				.Return("Nothing")
				.WithStatus(HttpStatusCode.Unauthorized);

			var statusResult = await _httpClient.GetStringAsync($"{_hostUrl}/api2/status");
			Assert.That(statusResult, Is.EqualTo("Hello"));

			var echoResponse = await _httpClient.GetAsync($"{_hostUrl}/api2/echo");
			Assert.That(echoResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

			var echo2Response = await _httpClient.GetAsync($"{_hostUrl}/api2/echo2");
			Assert.That(echo2Response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
		}


		[Test]
		public async Task Should_hit_the_same_url_multiple_times()
		{
			string endpoint = _hostUrl;
			_stubHttp = HttpMockRepository.At(endpoint);


			_stubHttp
				.Stub(x => x.Get("/api2/echo"))
				.Return("Echo")
				.NotFound();

			_stubHttp
				.Stub(x => x.Get("/api2/echo2"))
				.Return("Nothing")
				.WithStatus(HttpStatusCode.Unauthorized);


			for (int count = 0; count < 6; count++)
			{
				await RequestEcho(endpoint);
			}
		}

		[Test]
		public async Task Should_support_range_requests()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);
			string query = "/path/file";
			int fileSize = 2048;
			string pathToFile = CreateFile(fileSize);

			try
			{
				_stubHttp.Stub(x => x.Get(query))
					.ReturnFileRange(pathToFile, 0, 1023)
					.WithStatus(HttpStatusCode.PartialContent);

				var request = new HttpRequestMessage(HttpMethod.Get, _hostUrl + query);
				request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(0, 1023);
				var response = await _httpClient.SendAsync(request);
				var downloadData = await response.Content.ReadAsByteArrayAsync();

				Assert.That(downloadData.Length, Is.EqualTo(1024));
			}
			finally
			{
				try
				{
					File.Delete(pathToFile);
				}
				catch
				{
				}
			}
		}

		[Test]
		public async Task SUT_should_return_stubbed_response_for_custom_verbs()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			const string expected = "<xml><>response>Hello World</response></xml>";
			_stubHttp.Stub(x => x.CustomVerb("/endpoint", "PURGE"))
				.Return(expected)
				.OK();

			var request = new HttpRequestMessage(new HttpMethod("PURGE"), $"{_hostUrl}/endpoint");
			request.Headers.Host = "nonstandard.host";
			request.Headers.Add("X-Go-Faster", "11");
			var response = await _httpClient.SendAsync(request);
			var responseBody = await response.Content.ReadAsStringAsync();
			Assert.That(responseBody, Is.EqualTo(expected));
		}

		[TestCase(1)]
		[TestCase(10)]
		[TestCase(50)]
		public async Task SUT_should_get_back_the_collection_of_requests(int count)
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			var requestHandlerStub = _stubHttp.Stub(x => x.Post("/endpoint"));

			requestHandlerStub.Return(string.Empty).OK();

			var expectedBodyList = new List<string>();
			for (int i = 0; i < count; i++)
			{
				string expected = string.Format("<xml><>response>{0}</response></xml>",
					string.Join(" ", Enumerable.Range(0, i)));
				await _httpClient.PostAsync($"{_hostUrl}/endpoint",
					new StringContent(expected, Encoding.UTF8, "application/xml"));
				expectedBodyList.Add(expected);
			}

			var requestHandler = (RequestHandler) requestHandlerStub;

			var observedRequests = requestHandler.GetObservedRequests();
			Assert.That(observedRequests.ToList().Count, Is.EqualTo(expectedBodyList.Count));

			for (int i = 0; i < expectedBodyList.Count; i++)
			{
				Assert.That(observedRequests.ElementAt(i).Body, Is.EqualTo(expectedBodyList.ElementAt(i)));
			}
		}

		[TestCase(500, 200)]
		[TestCase(1234, 250)]
		[TestCase(2000, 350)]
		public async Task Should_wait_more_than_the_added_delay(int wait, int added)
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);
			var stub = _stubHttp.Stub(x => x.Get("/endpoint")).Return("Delayed response");
			stub.OK();

			stub.WithDelay(wait);
			var sw = new Stopwatch();

			sw.Start();
			var ans = await _httpClient.GetStringAsync($"{_hostUrl}/endpoint");
			sw.Stop();

			Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(wait));
			Assert.That(ans, Is.EqualTo("Delayed response"));

			stub.WithDelay(TimeSpan.FromMilliseconds(added)).Return("Delayed response 2");
			sw.Reset();

			sw.Start();
			ans = await _httpClient.GetStringAsync($"{_hostUrl}/endpoint");
			sw.Stop();

			Assert.That(sw.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(wait + added));
			Assert.That(ans, Is.EqualTo("Delayed response 2"));
		}

		[TestCase(407, 50)]
		[TestCase(1015, 50)]
		[TestCase(1691, 50)]
		public async Task Delayed_stub_shouldnt_block_undelayed_stub(int wait, int epsilon)
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);
			_stubHttp.Stub(x => x.Get("/firstEndp")).WithDelay(wait).Return("Delayed response (stub 1)").OK();
			_stubHttp.Stub(x => x.Get("/secondEndp")).Return("Undelayed response (stub 2)").OK();

			Stopwatch swDelayed = new Stopwatch();
			Stopwatch swUndelayed = new Stopwatch();

			// This triggers the server so that we won't have any initial (unwanted) delays
			await _httpClient.GetStringAsync($"{_hostUrl}/firstEndp");
			await _httpClient.GetStringAsync($"{_hostUrl}/secondEndp");

			var taskDelayed = Task.Run(async () =>
			{
				swDelayed.Start();
				var ans = await _httpClient.GetStringAsync($"{_hostUrl}/firstEndp");
				swDelayed.Stop();
				return ans;
			});

			var taskUndelayed = Task.Run(async () =>
			{
				swUndelayed.Start();
				var ans = await _httpClient.GetStringAsync($"{_hostUrl}/secondEndp");
				swUndelayed.Stop();
				return ans;
			});

			await Task.WhenAll(taskDelayed, taskUndelayed);

			Assert.That(taskDelayed.Result, Is.EqualTo("Delayed response (stub 1)"));
			Assert.That(taskUndelayed.Result, Is.EqualTo("Undelayed response (stub 2)"));
			Assert.That(swDelayed.ElapsedMilliseconds - epsilon, Is.GreaterThan(swUndelayed.ElapsedMilliseconds));
		}

		private string CreateFile(int fileSize)
		{
			string fileName = Path.GetTempFileName();
			using (FileStream fileStream = File.OpenWrite(fileName))
			{
				for (int count = 0; count < fileSize; count++)
				{
					fileStream.WriteByte((byte) count);
				}

				fileStream.Close();
			}

			return fileName;
		}

		private static async Task RequestEcho(string endpoint)
		{
			var response = await _httpClient.GetAsync(endpoint + "/api2/echo");
			Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
		}
	}
}
