using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingMultipleServersTests
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		[Test, Repeat(3)]
		public async Task Should_stubs_on_different_ports_each_time()
		{
			string expected = "expected response";
			var hostUrl = HostHelper.GenerateAHostUrlForAStubServerWith("app");

			HttpMockRepository.At(hostUrl)
				.Stub(x => x.Get("/app/endpoint"))
				.Return(expected)
				.OK();

			var result = await _httpClient.GetStringAsync($"{hostUrl}/endpoint");
			Assert.That(result, Is.EqualTo(expected));
		}
	}
}