using System;
using System.IO;
using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class EndpointsReturningFilesTests	
	{
		private IStubHttp _stubHttp;
		private const string FILE_NAME = "transcode-input.mp3";
		private const string RES_TRANSCODE_INPUT_MP3 = "./res/"+FILE_NAME;

		[TestFixtureSetUp]
		public void SetUp() {
			_stubHttp = HttpMockRepository.At("http://localhost:9191/");
			_stubHttp.Stub(x => x.Get("/afile"))
				.ReturnFile(RES_TRANSCODE_INPUT_MP3)
				.OK();

			_stubHttp.Stub(x => x.Get("/afile2"))
				.ReturnFile(RES_TRANSCODE_INPUT_MP3)
				.OK();
		}

		[Test]
		public void Setting_return_file_should_add_correct_content_disposition_header() {
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:9191/afile");
			using (var webResponse = httpWebRequest.GetResponse()) {
				Assert.That(webResponse.Headers["Content-Disposition"], Is.Not.Null);
				Assert.That(webResponse.Headers["Content-Disposition"], Is.EqualTo(string.Format("attachment; filename=\"{0}\"", FILE_NAME)));
			}
		}

		[Test]
		public void Can_access_a_differnet_stub_pointing_to_the_same_physical_file() {
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:9191/afile2");
			using (var webResponse = httpWebRequest.GetResponse()) {
				Assert.That(webResponse.Headers["Content-Disposition"], Is.Not.Null);
				Assert.That(webResponse.Headers["Content-Disposition"], Is.EqualTo(string.Format("attachment; filename=\"{0}\"", FILE_NAME)));
			}
		}

		[Test]
		public void Should_allow_reading_of_file_into_another_stream() {
			var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://localhost:9191/afile");
			using (var webResponse = httpWebRequest.GetResponse()) {
				var responseStream = webResponse.GetResponseStream();
				Assert.That(responseStream, Is.Not.Null);
				Assert.That(responseStream.CanRead);
				
				var fileInfo = new FileInfo(RES_TRANSCODE_INPUT_MP3);
				var fileLength = fileInfo.Length;

				foreach (string key in webResponse.Headers.Keys) {
					Console.WriteLine("{0}: {1}", key, webResponse.Headers[key]);
				}
				
				Assert.That(webResponse.ContentLength, Is.EqualTo(fileLength));
				using (var sr = new StreamReader(responseStream)) {
					var readToEnd = sr.ReadToEnd();
					Assert.That(readToEnd.Length, Is.EqualTo(fileLength));
				}
			}
		}
	}
}