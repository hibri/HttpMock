using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpExpectationTests
	{
		[Test]
		public void Should_assert_a_request_was_made() {
			var stubHttp = HttpMockRepository.At("http://localhost:90");
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();

			new WebClient().DownloadString("http://localhost:90/api/status");

			stubHttp.AssertWasCalled(x => x.Get("/api/status"));

		}

		[Test]
		public void Should_assert_that_a_request_was_not_made()
		{
			var stubHttp = HttpMockRepository.At("http://localhost:90");
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();
			stubHttp.Stub(x => x.Get("/api/echo")).Return("OK").OK();

			new WebClient().DownloadString("http://localhost:90/api/status");
			

			stubHttp.AssertWasNotCalled(x => x.Get("/api/echo"));

		}

		[Test]
		public void Should_match_a_POST_request_was_made_with_the_expected_body() {

			var stubHttp = HttpMockRepository.At("http://localhost:90");
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "postdata";
			new WebClient().UploadString("http://localhost:90/endpoint/handler", expectedData);

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).WithBody(expectedData) ;
		}


	}
}