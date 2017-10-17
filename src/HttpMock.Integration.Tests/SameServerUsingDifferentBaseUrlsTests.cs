using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class SameServerUsingDifferentBaseUrlsTests
	{
		private string _hostUrl;
	    private IHttpServer stubHttp;

        [OneTimeSetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		    stubHttp = HttpMockRepository.At(_hostUrl);
		    stubHttp.IsAvailable();
		}

		[Test]
		public void UsingAppOne()
		{
			string expected = "expected response";
			var url = _hostUrl + "/appone";
		    stubHttp
                .Stub(x => x.Get("/appone/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();

			Assert.That(wc.DownloadString(string.Format("{0}/endpoint", url)), Is.EqualTo(expected));
		}

		[Test]
		public void UsingAppTwo()
		{
			string expected = "expected response";

			var url = _hostUrl + "/apptwo";
			stubHttp
				.Stub(x => x.Get("/apptwo/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();
			Assert.That(wc.DownloadString(string.Format("{0}/endpoint", url)), Is.EqualTo(expected));
		}

		[Test]
		public void UsingAppThree()
		{
			string expected = "expected response";
			var url = _hostUrl + "/appthree";
		    stubHttp
                .Stub(x => x.Get("/appthree/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();
			Assert.That(wc.DownloadString(string.Format("{0}/endpoint", url)), Is.EqualTo(expected));
		}
	}
}