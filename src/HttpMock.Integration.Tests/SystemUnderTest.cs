using System.Net.Http;

namespace HttpMock.Integration.Tests
{
	public class SystemUnderTest
	{
		private static readonly HttpClient _httpClient = new HttpClient();

		public string GetData(string endpoint) {
			return _httpClient.GetStringAsync(endpoint).GetAwaiter().GetResult();
		}
	}
}