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
		private const string FILE_NAME = "transcode-input.mp3";
		private const string RES_TRANSCODE_INPUT_MP3 = "./res/"+FILE_NAME;

		[Test]
		public void A_Setting_return_file_return_the_correct_content_length() {
			var stubHttp = HttpMockRepository.At("http://localhost.:9191/");
			stubHttp.Stub(x => x.Get("/afile"))
				.ReturnFile(RES_TRANSCODE_INPUT_MP3)
				.OK();

			Console.WriteLine(stubHttp.WhatDoIHave());

			var fileLength = new FileInfo(RES_TRANSCODE_INPUT_MP3).Length;

			var webRequest = (HttpWebRequest) WebRequest.Create("http://localhost.:9191/afile");
			using (var response = webRequest.GetResponse())
			using(var responseStream = response.GetResponseStream())
			{

				var bytes = new byte[response.ContentLength];
				responseStream.Read(bytes, 0, (int) response.ContentLength);
				
				Assert.That(response.ContentLength, Is.EqualTo(fileLength));
			}
		}

		[Test]
		public void C_Setting_return_file_should_add_correct_content_disposition_header() {
			var stubHttp = HttpMockRepository.At("http://localhost.:9191/");
			stubHttp.Stub(x => x.Get("/afile1"))
				.ReturnFile(RES_TRANSCODE_INPUT_MP3)
				.OK();

			Console.WriteLine(stubHttp.WhatDoIHave());

			var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost.:9191/afile1");
			using (var response = webRequest.GetResponse())
			using (response.GetResponseStream()) {
				Assert.That(response.Headers["Content-Disposition"], Is.Not.Null);
				Assert.That(response.Headers["Content-Disposition"], Is.EqualTo(string.Format("attachment; filename=\"{0}\"", FILE_NAME)));
			}

		}

		[Test]
		public void B_Can_access_a_differnet_stub_pointing_to_the_same_physical_file() {
			var stubHttp = HttpMockRepository.At("http://localhost.:9191/");
			stubHttp.Stub(x => x.Get("/afile2"))
				.ReturnFile(RES_TRANSCODE_INPUT_MP3)
				.OK();

			Console.WriteLine(stubHttp.WhatDoIHave());

			var webRequest = (HttpWebRequest)WebRequest.Create("http://localhost.:9191/afile2");
			using(var response = webRequest.GetResponse())
			using(response.GetResponseStream()) {
				Assert.That(response.Headers["Content-Disposition"], Is.Not.Null);
				Assert.That(response.Headers["Content-Disposition"], Is.EqualTo(string.Format("attachment; filename=\"{0}\"", FILE_NAME)));
			}
		}
	}
}