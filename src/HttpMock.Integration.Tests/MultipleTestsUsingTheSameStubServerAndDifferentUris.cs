using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class MultipleTestsUsingTheSameStubServerAndDifferentUris
	{
		[SetUp]
		public void SetUp()
		{
		    var hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		    TestContext.CurrentContext.SetCurrentHostUrl(hostUrl);
		    TestContext.CurrentContext.SetCurrentHttpMock(HttpMockRepository.At(hostUrl));
        }

		[Test]
		public void FirstTest()
		{
			var wc = new WebClient();
			string stubbedReponse = "Response for first test";
			var stubHttp = TestContext.CurrentContext.GetCurrentHttpMock()
                .WithNewContext();

			stubHttp
				.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponse)
				.OK();

			Assert.That(wc.UploadString(string.Format("{0}/firsttest/", TestContext.CurrentContext.GetCurrentHostUrl()), "x"), Is.EqualTo(stubbedReponse));
		}

		[Test]
		public void SecondTest()
		{
			var wc = new WebClient();
			string stubbedReponse = "Response for second test";
		    TestContext.CurrentContext.GetCurrentHttpMock()
                .WithNewContext()
				.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponse)
				.OK();

			Assert.That(wc.UploadString(string.Format("{0}/secondtest/", TestContext.CurrentContext.GetCurrentHostUrl()), "x"), Is.EqualTo(stubbedReponse));
		}

		[Test]
		public void Stubs_should_be_unique_within_context()
		{
			var wc = new WebClient();
			string stubbedReponseOne = "Response for first test in context";
			string stubbedReponseTwo = "Response for second test in context";

			IHttpServer stubHttp = TestContext.CurrentContext.GetCurrentHttpMock().WithNewContext();

			stubHttp.Stub(x => x.Post("/firsttest"))
				.Return(stubbedReponseOne)
				.OK();

			stubHttp.Stub(x => x.Post("/secondtest"))
				.Return(stubbedReponseTwo)
				.OK();

			Assert.That(wc.UploadString(string.Format("{0}/firsttest/", TestContext.CurrentContext.GetCurrentHostUrl()), "x"), Is.EqualTo(stubbedReponseOne));
			Assert.That(wc.UploadString(string.Format("{0}/secondtest/", TestContext.CurrentContext.GetCurrentHostUrl()), "x"), Is.EqualTo(stubbedReponseTwo));
		}
	}
}