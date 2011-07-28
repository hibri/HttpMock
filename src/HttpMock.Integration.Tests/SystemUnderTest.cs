using System.Net;

namespace SevenDigital.HttpMock.Integration.Tests
{
	public class SystemUnderTest
	{
		public string GetData() {
			return new WebClient().DownloadString("http://localhost:8080/someapp/someendpoint");
		}
	}
}