using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class SameServerUsingDifferentBaseUrlsTests
	{
		private string _hostUrl;

		[OneTimeSetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		}

		[Test]
		public void UsingAppOne()
		{
			string expected = "expected response";
			var url = _hostUrl + "/appone";
			HttpMockRepository.At(url)
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
			HttpMockRepository.At(url)
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
			HttpMockRepository.At(url)
				.Stub(x => x.Get("/appthree/endpoint"))
				.Return(expected)
				.OK();

			WebClient wc = new WebClient();
			Assert.That(wc.DownloadString(string.Format("{0}/endpoint", url)), Is.EqualTo(expected));
		}
	}
}