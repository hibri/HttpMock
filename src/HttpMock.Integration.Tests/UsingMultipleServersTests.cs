using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class UsingMultipleServersTests
	{
		[Test, Repeat(3)]
		public void Should_stubs_on_different_ports_each_time()
		{
			string expected = "expected response";
			var hostUrl = HostHelper.GenerateAHostUrlForAStubServerWith("app");

			HttpMockRepository.At(hostUrl)
				.Stub(x => x.Get("/app/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();

			Assert.That(wc.DownloadString(string.Format("{0}/endpoint", hostUrl)), Is.EqualTo(expected));
		}
	}
}