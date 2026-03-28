using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class SameServerUsingDifferentBaseUrlsTests
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private string _hostUrl;

		[OneTimeSetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		}

		[Test]
		public async Task UsingAppOne()
		{
			string expected = "expected response";
			var url = _hostUrl + "/appone";
			HttpMockRepository.At(url)
				.Stub(x => x.Get("/appone/endpoint"))
				.Return(expected)
				.OK();

			var result = await _httpClient.GetStringAsync($"{url}/endpoint");

			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public async Task UsingAppTwo()
		{
			string expected = "expected response";

			var url = _hostUrl + "/apptwo";
			HttpMockRepository.At(url)
				.Stub(x => x.Get("/apptwo/endpoint"))
				.Return(expected)
				.OK();

			var result = await _httpClient.GetStringAsync($"{url}/endpoint");
			Assert.That(result, Is.EqualTo(expected));
		}

		[Test]
		public async Task UsingAppThree()
		{
			string expected = "expected response";
			var url = _hostUrl + "/appthree";
			HttpMockRepository.At(url)
				.Stub(x => x.Get("/appthree/endpoint"))
				.Return(expected)
				.OK();

			var result = await _httpClient.GetStringAsync($"{url}/endpoint");
			Assert.That(result, Is.EqualTo(expected));
		}
	}
}