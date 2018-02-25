using System;
using System.IO;
using System.Linq;
using System.Net;
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
		private IHttpServer _stubHttp;
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		}

		[Test]
		public void SUT_should_return_stubbed_response()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			const string expected = "<xml><>response>Hello World</response></xml>";
			_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();



			string result = new WebClient().DownloadString(string.Format("{0}/endpoint", _hostUrl));

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public void SUT_should_return_stubbed_byte_array_response()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			byte[] expected = Encoding.UTF8.GetBytes("<xml><>response>Change to bytes to simulate possible stream response</response></xml>");
			_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

			byte[] result = new WebClient().DownloadData(string.Format("{0}/endpoint", _hostUrl));

			Assert.That(result, Is.EqualTo(expected));
		}

        [TestCase(1)]
        [TestCase(100)]
        [TestCase(5000)]
        public void SUT_should_get_back_exact_content_in_the_last_request_body(int count)
        {
            _stubHttp = HttpMockRepository.At(_hostUrl);

            string expected = string.Format("<xml><>response>{0}</response></xml>", string.Join(" ", Enumerable.Range(0, count)));
            var requestHandler = _stubHttp.Stub(x => x.Post("/endpoint"));

            requestHandler.Return(expected).OK();

            

            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/xml";
                wc.UploadString(string.Format("{0}/endpoint", _hostUrl), expected);
            }
            var requestBody = ((RequestHandler)requestHandler).LastRequest().Body;

            Assert.That(requestBody, Is.EqualTo(expected));
        }      


		[Test]
		public void Should_start_listening_before_stubs_have_been_set()
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

			string result = new WebClient().DownloadString(string.Format("{0}/endpoint", _hostUrl));

			Assert.That(result, Is.EqualTo("listening"));
		}

		[Test]
		public void Should_return_expected_ok_response()
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



			var wc = new WebClient();

			Assert.That(wc.DownloadString(string.Format("{0}/api2/status", _hostUrl)), Is.EqualTo("Hello"));

			try
			{
				Console.WriteLine(wc.DownloadString(_hostUrl + "/api2/echo"));
			}
			catch (Exception ex)
			{
				Assert.That(ex, Is.InstanceOf(typeof (WebException)));
				Assert.That(((WebException) ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}

			try
			{
				wc.DownloadString(_hostUrl + "/api2/echo2");
			}
			catch (Exception ex)
			{
				Assert.That(ex, Is.InstanceOf(typeof (WebException)));
				Assert.That(((WebException) ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}
		}


		[Test]
		public void Should_hit_the_same_url_multiple_times()
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
				RequestEcho(endpoint);
			}
		}

		[Test]
		public void Should_support_range_requests()
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



				HttpWebRequest request = (HttpWebRequest) HttpWebRequest.Create(_hostUrl + query);
				request.Method = "GET";
				request.AddRange(0, 1023);
				HttpWebResponse response = (HttpWebResponse) request.GetResponse();
				byte[] downloadData = new byte[response.ContentLength];
				using (response)
				{
					response.GetResponseStream().Read(downloadData, 0, downloadData.Length);
				}
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
		public void SUT_should_return_stubbed_response_for_custom_verbs()
		{
			_stubHttp = HttpMockRepository.At(_hostUrl);

			const string expected = "<xml><>response>Hello World</response></xml>";
			_stubHttp.Stub(x => x.CustomVerb("/endpoint", "PURGE"))
				.Return(expected)
				.OK();



			var request = (HttpWebRequest) WebRequest.Create(string.Format("{0}/endpoint", _hostUrl));
			request.Method = "PURGE";
			request.Host = "nonstandard.host";
			request.Headers.Add("X-Go-Faster", "11");
			using (var response = request.GetResponse())
			using (var stream = response.GetResponseStream())
			{
				var responseBody = new StreamReader(stream).ReadToEnd();
				Assert.That(responseBody, Is.EqualTo(expected));
			}
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

		private static void RequestEcho(string endpoint)
		{
			var wc = new WebClient();

			try
			{
				wc.DownloadString(endpoint + "/api2/echo");
			}
			catch (Exception ex)
			{
				Assert.That(ex, Is.InstanceOf(typeof (WebException)));
				Assert.That(((WebException) ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
			}
		}
		
		[TestCase(1)]
        [TestCase(10)]
        [TestCase(50)]
        public void SUT_should_get_back_the_collection_of_requests(int count)
        {
            _stubHttp = HttpMockRepository.At(_hostUrl);

            var requestHandlerStub = _stubHttp.Stub(x => x.Post("/endpoint"));

            requestHandlerStub.Return(string.Empty).OK();

            var expectedBodyList = new List<string>();
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/xml";
                for (int i = 0; i < count; i++)
                {
                    string expected = string.Format("<xml><>response>{0}</response></xml>", string.Join(" ", Enumerable.Range(0, i)));
                    wc.UploadString(string.Format("{0}/endpoint", _hostUrl), expected);
                    expectedBodyList.Add(expected);
                }
            }

            var requestHandler = (RequestHandler)requestHandlerStub;

            var observedRequests = requestHandler.GetObservedRequests();
            Assert.AreEqual(expectedBodyList.Count, observedRequests.ToList().Count);

            for (int i = 0; i < expectedBodyList.Count; i++)
            {
                Assert.AreEqual(expectedBodyList.ElementAt(i), observedRequests.ElementAt(i).Body);
            }
        }

        [TestCase(500, 200)]
        [TestCase(1234, 250)]
        [TestCase(2000, 350)]
        public void Should_wait_more_than_the_added_delay(int wait, int added)
        {
            _stubHttp = HttpMockRepository.At(_hostUrl);
            var stub = _stubHttp.Stub(x => x.Get("/endpoint")).Return("Delayed response");
            stub.OK();

            stub.WithDelay(wait);
            string ans;
            var sw = new Stopwatch();

            using (var wc = new WebClient())
            {
                sw.Start();
                ans = wc.DownloadString($"{_hostUrl}/endpoint");
                sw.Stop();
            }

            Assert.GreaterOrEqual(sw.ElapsedMilliseconds, wait);
            Assert.AreEqual("Delayed response", ans);

            stub.WithDelay(TimeSpan.FromMilliseconds(added)).Return("Delayed response 2");
            sw.Reset();
            using (var wc = new WebClient())
            {
                sw.Start();
                ans = wc.DownloadString($"{_hostUrl}/endpoint");
                sw.Stop();
            }

            Assert.GreaterOrEqual(sw.ElapsedMilliseconds, wait + added);
            Assert.AreEqual("Delayed response 2", ans);

        }


        public async Task<string> StartDelayedRequest(Stopwatch sw)
        {
            using (var wc = new WebClient())
            {
                sw.Restart();
                var ans = await wc.DownloadStringTaskAsync($"{_hostUrl}/firstEndp");
                sw.Stop();
                return ans;
            }
        }

        [TestCase(407, 50)]
        [TestCase(1015, 50)]
        [TestCase(1691, 50)]
        public void Delayed_stub_shouldnt_block_undelayed_stub(int wait, int epsilon)
        {
            _stubHttp = HttpMockRepository.At(_hostUrl);
            _stubHttp.Stub(x => x.Get("/firstEndp")).WithDelay(wait).Return("Delayed response (stub 1)").OK();
            _stubHttp.Stub(x => x.Get("/secondEndp")).Return("Undelayed response (stub 2)").OK();

            Stopwatch swDelayed = new Stopwatch();
            Stopwatch swUndelayed = new Stopwatch();
            Task<string> taskDelayed = null, 
                taskUndelayed = null;

            using (var wcUndelayed = new WebClient())
            using (var wcDelayed = new WebClient())
            {

                // This triggers the server so that we won't have any initial (unwanted) delays
	        
                wcDelayed.DownloadString($"{_hostUrl}/firstEndp");
	            wcUndelayed.DownloadString($"{_hostUrl}/secondEndp");

                taskDelayed = StartDelayedRequest(swDelayed);

                taskUndelayed = Task.Run(() =>
                {
                    swUndelayed.Start();
                    var ans = wcUndelayed.DownloadString($"{_hostUrl}/secondEndp");
                    swUndelayed.Stop();
                    return ans;
                });

                taskDelayed.Wait();
                taskUndelayed.Wait();
            }

            Assert.AreEqual("Delayed response (stub 1)", taskDelayed.Result);
            Assert.AreEqual("Undelayed response (stub 2)", taskUndelayed.Result);
            Assert.Greater(swDelayed.ElapsedMilliseconds - epsilon, swUndelayed.ElapsedMilliseconds);

        }

    }
}