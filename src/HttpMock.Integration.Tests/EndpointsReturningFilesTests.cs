using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class EndpointsReturningFilesTests	
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private const string FILE_NAME = "transcode-input.mp3";
		private static readonly string RES_TRANSCODE_INPUT_MP3 = Path.Combine("res",FILE_NAME);

		[Test]
		public async Task A_Setting_return_file_return_the_correct_content_length() {
			var stubHttp = HttpMockRepository.At("http://localhost.:9191");


		    var pathToFile = Path.Combine(TestContext.CurrentContext.TestDirectory, RES_TRANSCODE_INPUT_MP3);

		    stubHttp.Stub(x => x.Get("/afile"))
				.ReturnFile(pathToFile)
				.OK();

			Console.WriteLine(stubHttp.WhatDoIHave());
            
			var fileLength = new FileInfo(pathToFile).Length;

			var bytes = await _httpClient.GetByteArrayAsync("http://localhost.:9191/afile");

			Assert.That((long)bytes.Length, Is.EqualTo(fileLength));
		}

	}
}