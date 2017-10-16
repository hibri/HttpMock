using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using System.Collections.Generic;

namespace HttpMock.Integration.Tests
{
    [TestFixture]
    public class HttpEndPointTests
    {
        [SetUp]
        public void SetUp()
        {
            TestContext.CurrentContext.SetCurrentHostUrl(HostHelper.GenerateAHostUrlForAStubServer());
        }

        [Test]
        public void SUT_should_return_stubbed_response()
        {
            
            IHttpServer _stubHttp = HttpMockRepository.At(TestContext.CurrentContext.GetCurrentHostUrl());

            const string expected = "<xml><>response>Hello World</response></xml>";
            _stubHttp.Stub(x => x.Get("/endpoint"))
                .Return(expected)
                .OK();



            string result = new WebClient().DownloadString(string.Format("{0}/endpoint", TestContext.CurrentContext.GetCurrentHostUrl()));

            Assert.That(result, Is.EqualTo(expected));
        }

        [TestCase(1)]
        [TestCase(100)]
        [TestCase(5000)]
        public void SUT_should_get_back_exact_content_in_the_last_request_body(int count)
        {
            IHttpServer _stubHttp = HttpMockRepository.At(TestContext.CurrentContext.GetCurrentHostUrl());

            string expected = string.Format("<xml><>response>{0}</response></xml>", string.Join(" ", Enumerable.Range(0, count)));
            var requestHandler = _stubHttp.Stub(x => x.Post("/endpoint"));

            requestHandler.Return(expected).OK();



            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/xml";
                wc.UploadString(string.Format("{0}/endpoint", TestContext.CurrentContext.GetCurrentHostUrl()), expected);
            }
            var requestBody = ((RequestHandler)requestHandler).LastRequest().Body;

            Assert.That(requestBody, Is.EqualTo(expected));
        }


        [Test]
        public void Should_start_listening_before_stubs_have_been_set()
        {
            IHttpServer _stubHttp = HttpMockRepository.At(TestContext.CurrentContext.GetCurrentHostUrl());

            _stubHttp.Stub(x => x.Get("/endpoint"))
                .Return("listening")
                .OK();

            using (var tcpClient = new TcpClient())
            {
                var uri = new Uri(TestContext.CurrentContext.GetCurrentHostUrl());

                tcpClient.Connect(uri.Host, uri.Port);

                Assert.That(tcpClient.Connected, Is.True);

                tcpClient.Close();
            }

            string result = new WebClient().DownloadString(string.Format("{0}/endpoint", TestContext.CurrentContext.GetCurrentHostUrl()));

            Assert.That(result, Is.EqualTo("listening"));
        }

        [Test]
        public void Should_return_expected_ok_response()
        {
            IHttpServer _stubHttp = HttpMockRepository.At(TestContext.CurrentContext.GetCurrentHostUrl());

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

            Assert.That(wc.DownloadString(string.Format("{0}/api2/status", TestContext.CurrentContext.GetCurrentHostUrl())), Is.EqualTo("Hello"));

            try
            {
                Console.WriteLine(wc.DownloadString(TestContext.CurrentContext.GetCurrentHostUrl() + "/api2/echo"));
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf(typeof(WebException)));
                Assert.That(((WebException)ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
            }

            try
            {
                wc.DownloadString(TestContext.CurrentContext.GetCurrentHostUrl() + "/api2/echo2");
            }
            catch (Exception ex)
            {
                Assert.That(ex, Is.InstanceOf(typeof(WebException)));
                Assert.That(((WebException)ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
            }
        }


        [Test]
        public void Should_hit_the_same_url_multiple_times()
        {
            string endpoint = TestContext.CurrentContext.GetCurrentHostUrl();
            IHttpServer _stubHttp = HttpMockRepository.At(endpoint);


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
            IHttpServer _stubHttp = HttpMockRepository.At(TestContext.CurrentContext.GetCurrentHostUrl());
            string query = "/path/file";
            int fileSize = 2048;
            string pathToFile = CreateFile(fileSize);

            try
            {
                _stubHttp.Stub(x => x.Get(query))
                    .ReturnFileRange(pathToFile, 0, 1023)
                    .WithStatus(HttpStatusCode.PartialContent);



                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(TestContext.CurrentContext.GetCurrentHostUrl() + query);
                request.Method = "GET";
                request.AddRange(0, 1023);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
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
            IHttpServer _stubHttp = HttpMockRepository.At(TestContext.CurrentContext.GetCurrentHostUrl());

            const string expected = "<xml><>response>Hello World</response></xml>";
            _stubHttp.Stub(x => x.CustomVerb("/endpoint", "PURGE"))
                .Return(expected)
                .OK();



            var request = (HttpWebRequest)WebRequest.Create(string.Format("{0}/endpoint", TestContext.CurrentContext.GetCurrentHostUrl()));
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
                    fileStream.WriteByte((byte)count);
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
                Assert.That(ex, Is.InstanceOf(typeof(WebException)));
                Assert.That(((WebException)ex).Status, Is.EqualTo(WebExceptionStatus.ProtocolError));
            }
        }

        [TestCase(1)]
        [TestCase(10)]
        [TestCase(50)]
        public void SUT_should_get_back_the_collection_of_requests(int count)
        {
            IHttpServer _stubHttp = HttpMockRepository.At(TestContext.CurrentContext.GetCurrentHostUrl());

            var requestHandlerStub = _stubHttp.Stub(x => x.Post("/endpoint"));

            requestHandlerStub.Return(string.Empty).OK();

            var expectedBodyList = new List<string>();
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/xml";
                for (int i = 0; i < count; i++)
                {
                    string expected = string.Format("<xml><>response>{0}</response></xml>", string.Join(" ", Enumerable.Range(0, i)));
                    wc.UploadString(string.Format("{0}/endpoint", TestContext.CurrentContext.GetCurrentHostUrl()), expected);
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
    }
}