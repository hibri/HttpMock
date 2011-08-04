using System.Net;

namespace SevenDigital.HttpMock.Integration.Tests
{
	public class SystemUnderTest
	{
		public string GetData(string endpoint) {
			return new WebClient().DownloadString(endpoint);
		}
	}
}