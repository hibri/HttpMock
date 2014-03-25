using System;
using System.Net;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpExpectationTests
	{
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		}

		[Test]
		public void Should_assert_a_request_was_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();

			new WebClient().DownloadString(string.Format("{0}/api/status", _hostUrl));

			stubHttp.AssertWasCalled(x => x.Get("/api/status"));
		}


		[Test]
		public void Should_assert_that_a_request_was_not_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();
			stubHttp.Stub(x => x.Get("/api/echo")).Return("OK").OK();

			new WebClient().DownloadString(string.Format("{0}/api/status", _hostUrl));

			stubHttp.AssertWasNotCalled(x => x.Get("/api/echo"));
		}

		[Test]
		public void Should_assert_when_stub_is_missing()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasCalled(x => x.Get("/api/echo")));
		}

		[Test]
		public void Should_match_a_POST_request_was_made_with_the_expected_body()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "postdata";

			new WebClient().UploadString(string.Format("{0}/endpoint/handler", _hostUrl), expectedData);

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).WithBody(expectedData);
		}

		[Test]
		public void Should_match_a_POST_request_was_made_with_a_body_that_matches_a_constraint()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "postdata" + DateTime.Now;

			new WebClient().UploadString(string.Format("{0}/endpoint/handler", _hostUrl), expectedData);

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).WithBody(Is.StringStarting("postdata"));
		}


		[Test]
		public void Should_not_match_a_POST_request_was_made_with_a_body_that_doesnt_match_a_constraint()
		{
			var stubHttp = HttpMockRepository.At("http://localhost:90");
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "DUMMYPREFIX-postdata" + DateTime.Now;
			new WebClient().UploadString("http://localhost:90/endpoint/handler", expectedData);

			Assert.Throws<AssertionException>(() =>
				stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler"))
					.WithBody(Is.StringStarting("postdata")));
		}

		[Test]
		public void Should_fail_assertion_if_request_header_is_missing()
		{
			const string endPoint = "/put/no/header";
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Put(endPoint)).Return("OK").OK();

			var request = (HttpWebRequest) WebRequest.Create(_hostUrl + endPoint);
			request.Method = "PUT";

			using (request.GetResponse())
			{
				Assert.Throws<AssertionException>(() =>
					stubHttp.AssertWasCalled(x => x.Put(endPoint))
						.WithHeader("X-Wibble", Is.EqualTo("Wobble")));
			}
		}

		[Test]
		public void Should_fail_assertion_if_request_header_differs_from_expectation()
		{
			const string endPoint = "/put/no/header";
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Put(endPoint)).Return("OK").OK();

			var request = (HttpWebRequest) WebRequest.Create(_hostUrl + endPoint);
			request.Method = "PUT";
			request.Headers.Add("Waffle", "Pancake");

			using (request.GetResponse())
			{
				Assert.Throws<AssertionException>(() =>
					stubHttp.AssertWasCalled(x => x.Put(endPoint))
						.WithHeader("Waffle", Is.EqualTo("Wobble")));
			}
		}

		[Test]
		public void Should_pass_assertion_if_request_header_satisfies_expectation()
		{
			const string endPoint = "/put/no/header";
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Put(endPoint)).Return("OK").OK();

			var request = (HttpWebRequest) WebRequest.Create(_hostUrl + endPoint);
			request.Method = "PUT";
			const string pancake = "Pancake";
			request.Headers.Add("Waffle", pancake);

			using (request.GetResponse())
				stubHttp.AssertWasCalled(x => x.Put(endPoint)).WithHeader("Waffle", Is.EqualTo(pancake));
		}

		[Test]
		public void Should_match_many_POST_requests_which_were_made_with_expected_body()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			const string expectedData = "postdata";
			new WebClient().UploadString(string.Format("{0}/endpoint/handler", _hostUrl), expectedData);
			new WebClient().UploadString(string.Format("{0}/endpoint/handler", _hostUrl), expectedData);

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).Times(2);
		}

		[Test]
		public void Should_not_match_if_times_value_doesnt_match_requestCount()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			const string expectedData = "postdata";

			new WebClient().UploadString(string.Format("{0}/endpoint/handler", _hostUrl), expectedData);
			new WebClient().UploadString(string.Format("{0}/endpoint/handler", _hostUrl), expectedData);

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).Times(3));
		}


		[Test]
		public void Should_assert_a_request_was_not_made_when_multiple_requests_are_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();
			stubHttp.Stub(x => x.Get("/api/echo")).Return("OK").OK();

			new WebClient().DownloadString(string.Format("{0}/api/status", _hostUrl));

			stubHttp.AssertWasNotCalled(x => x.Get("/api/echo"));

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasNotCalled(x => x.Get("/api/status")));
		}

		[Test]
		public void Should_assert_a_request_was_called_when_multiple_requests_are_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();
			stubHttp.Stub(x => x.Get("/api/echo")).Return("OK").OK();

			new WebClient().DownloadString(string.Format("{0}/api/status", _hostUrl));

			stubHttp.AssertWasCalled(x => x.Get("/api/status"));

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasCalled(x => x.Get("/api/echo")));
		}
	}
}