using System.Net;

namespace StubHttp
{
	public class SystemUnderTest
	{
		public string GetData() {
			return new WebClient().DownloadString("http://localhost:8080/someapp/someendpoint");
		}
	}
}