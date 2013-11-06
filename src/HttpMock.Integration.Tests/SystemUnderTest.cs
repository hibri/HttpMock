using System.Net;

namespace HttpMock.Integration.Tests
{
	public class SystemUnderTest
	{
		public string GetData(string endpoint) {
			return new WebClient().DownloadString(endpoint);
		}
	}
}