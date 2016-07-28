using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
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

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).WithBody(Does.StartWith("postdata"));
		}


		[Test]
		public void Should_not_match_a_POST_request_was_made_with_a_body_that_doesnt_match_a_constraint()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "DUMMYPREFIX-postdata" + DateTime.Now;
			new WebClient().UploadString(string.Format("{0}/endpoint/handler", _hostUrl), expectedData);

			Assert.Throws<AssertionException>(() =>
				stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler"))
					.WithBody(Does.StartWith("postdata")));
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

	    [Test]
	    public void Should_not_depend_on_the_order_the_stubs_were_created()
	    {
	        var expectedResponse = "PATH/ONE";

	        var stubHttp = HttpMockRepository.At(_hostUrl);

	        stubHttp.Stub(x => x.Get("/api/path")).Return("PATH").OK();
	        stubHttp.Stub(x => x.Get("/api/path/one")).Return(expectedResponse).OK();


	        var result = new WebClient().DownloadString(string.Format("{0}/api/path/one", _hostUrl));

            Assert.That(result, Is.EqualTo(expectedResponse));
	    }

        [Test]
        public void Should_assert_a_request_was_made_with_correct_query_params()
        {
            var stubHttp = HttpMockRepository.At(_hostUrl);
            var queryParameters = new Dictionary<string, string> {{"a", "a"}};
            stubHttp.Stub(x => x.Get("/api/status"))
                .WithParams(queryParameters)
                .Return("OK").OK();

            new WebClient().DownloadString(string.Format("{0}/api/status?a=a", _hostUrl));

            stubHttp.AssertWasCalled(x => x.Get("/api/status")).WithParams(queryParameters);
        }

        [Test]
        public void Should_fail_assertion_if_a_request_was_made_with_incorrect_query_params()
        {
            var stubHttp = HttpMockRepository.At(_hostUrl);
            var queryParams = new Dictionary<string, string> {{"a", "a"}};
            stubHttp.Stub(x => x.Get("/api/status"))
                .WithParams(queryParams)
                .Return("OK").OK();

            var webClient = new WebClient {QueryString = new NameValueCollection {{"a", "b"}}};

            using (webClient)
            {
                webClient.DownloadString(string.Format("{0}/api/status", _hostUrl));

                Assert.Throws<AssertionException>(() =>
                    stubHttp.AssertWasCalled(x => x.Get("/api/status")).WithParams(queryParams));
            }
        }

        [Test]
        public void Should_fail_assertion_if_a_request_was_made_with_no_query_params_but_there_should_have_been_there()
        {
            var stubHttp = HttpMockRepository.At(_hostUrl);
            var queryParams = new Dictionary<string, string> { { "a", "a" } };
            stubHttp.Stub(x => x.Get("/api/status"))
                .WithParams(queryParams)
                .Return("OK").OK();

            var webClient = new WebClient();

            using (webClient)
            {
                webClient.DownloadString(string.Format("{0}/api/status", _hostUrl));

                Assert.Throws<AssertionException>(() =>
                    stubHttp.AssertWasCalled(x => x.Get("/api/status")).WithParams(queryParams));
            }
        }
    }
}