using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using HttpMock.Verify.NUnit;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[TestFixture]
	public class HttpExpectationTests
	{
		private static readonly HttpClient _httpClient = new HttpClient();
		private string _hostUrl;

		[SetUp]
		public void SetUp()
		{
			_hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
		}

		[Test]
		public async Task Should_assert_a_request_was_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();

			await _httpClient.GetStringAsync($"{_hostUrl}/api/status");

			stubHttp.AssertWasCalled(x => x.Get("/api/status"));
		}


		[Test]
		public async Task Should_assert_that_a_request_was_not_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();
			stubHttp.Stub(x => x.Get("/api/echo")).Return("OK").OK();

			await _httpClient.GetStringAsync($"{_hostUrl}/api/status");

			stubHttp.AssertWasNotCalled(x => x.Get("/api/echo"));
		}

		[Test]
		public void Should_assert_when_stub_is_missing()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasCalled(x => x.Get("/api/echo")));
		}

		[Test]
		public async Task Should_match_a_POST_request_was_made_with_the_expected_body()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "postdata";

			await _httpClient.PostAsync($"{_hostUrl}/endpoint/handler",
				new StringContent(expectedData, Encoding.UTF8));

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).WithBody(expectedData);
		}

		[Test]
		public async Task Should_match_a_POST_request_was_made_with_a_body_that_matches_a_constraint()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "postdata" + DateTime.Now;

			await _httpClient.PostAsync($"{_hostUrl}/endpoint/handler",
				new StringContent(expectedData, Encoding.UTF8));

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).WithBody(Does.StartWith("postdata"));
		}


		[Test]
		public async Task Should_not_match_a_POST_request_was_made_with_a_body_that_doesnt_match_a_constraint()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			string expectedData = "DUMMYPREFIX-postdata" + DateTime.Now;
			await _httpClient.PostAsync($"{_hostUrl}/endpoint/handler",
				new StringContent(expectedData, Encoding.UTF8));

			Assert.Throws<AssertionException>(() =>
				stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler"))
					.WithBody(Does.StartWith("postdata")));
		}

		[Test]
		public async Task Should_fail_assertion_if_request_header_is_missing()
		{
			const string endPoint = "/put/no/header";
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Put(endPoint)).Return("OK").OK();

			var request = new HttpRequestMessage(HttpMethod.Put, _hostUrl + endPoint);
			request.Content = new StringContent("", Encoding.UTF8);

			using (await _httpClient.SendAsync(request))
			{
				Assert.Throws<AssertionException>(() =>
					stubHttp.AssertWasCalled(x => x.Put(endPoint))
						.WithHeader("X-Wibble", Is.EqualTo("Wobble")));
			}
		}

		[Test]
		public async Task Should_fail_assertion_if_request_header_differs_from_expectation()
		{
			const string endPoint = "/put/no/header";
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Put(endPoint)).Return("OK").OK();

			var request = new HttpRequestMessage(HttpMethod.Put, _hostUrl + endPoint);
			request.Content = new StringContent("", Encoding.UTF8);
			request.Headers.Add("Waffle", "Pancake");

			using (await _httpClient.SendAsync(request))
			{
				Assert.Throws<AssertionException>(() =>
					stubHttp.AssertWasCalled(x => x.Put(endPoint))
						.WithHeader("Waffle", Is.EqualTo("Wobble")));
			}
		}

		[Test]
		public async Task Should_pass_assertion_if_request_header_satisfies_expectation()
		{
			const string endPoint = "/put/no/header";
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Put(endPoint)).Return("OK").OK();

			var request = new HttpRequestMessage(HttpMethod.Put, _hostUrl + endPoint);
			request.Content = new StringContent("", Encoding.UTF8);
			const string pancake = "Pancake";
			request.Headers.Add("Waffle", pancake);

			using (await _httpClient.SendAsync(request))
				stubHttp.AssertWasCalled(x => x.Put(endPoint)).WithHeader("Waffle", Is.EqualTo(pancake));
		}

		[Test]
		public async Task Should_match_many_POST_requests_which_were_made_with_expected_body()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			const string expectedData = "postdata";
			await _httpClient.PostAsync($"{_hostUrl}/endpoint/handler",
				new StringContent(expectedData, Encoding.UTF8));
			await _httpClient.PostAsync($"{_hostUrl}/endpoint/handler",
				new StringContent(expectedData, Encoding.UTF8));

			stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).Times(2);
		}

		[Test]
		public async Task Should_not_match_if_times_value_doesnt_match_requestCount()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Post("/endpoint/handler")).Return("OK").OK();

			const string expectedData = "postdata";

			await _httpClient.PostAsync($"{_hostUrl}/endpoint/handler",
				new StringContent(expectedData, Encoding.UTF8));
			await _httpClient.PostAsync($"{_hostUrl}/endpoint/handler",
				new StringContent(expectedData, Encoding.UTF8));

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasCalled(x => x.Post("/endpoint/handler")).Times(3));
		}


		[Test]
		public async Task Should_assert_a_request_was_not_made_when_multiple_requests_are_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();
			stubHttp.Stub(x => x.Get("/api/echo")).Return("OK").OK();

			await _httpClient.GetStringAsync($"{_hostUrl}/api/status");

			stubHttp.AssertWasNotCalled(x => x.Get("/api/echo"));

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasNotCalled(x => x.Get("/api/status")));
		}

		[Test]
		public async Task Should_assert_a_request_was_called_when_multiple_requests_are_made()
		{
			var stubHttp = HttpMockRepository.At(_hostUrl);
			stubHttp.Stub(x => x.Get("/api/status")).Return("OK").OK();
			stubHttp.Stub(x => x.Get("/api/echo")).Return("OK").OK();

			await _httpClient.GetStringAsync($"{_hostUrl}/api/status");

			stubHttp.AssertWasCalled(x => x.Get("/api/status"));

			Assert.Throws<AssertionException>(() => stubHttp.AssertWasCalled(x => x.Get("/api/echo")));
		}

	    [Test]
	    public async Task Should_not_depend_on_the_order_the_stubs_were_created()
	    {
	        var expectedResponse = "PATH/ONE";

	        var stubHttp = HttpMockRepository.At(_hostUrl);

	        stubHttp.Stub(x => x.Get("/api/path")).Return("PATH").OK();
	        stubHttp.Stub(x => x.Get("/api/path/one")).Return(expectedResponse).OK();

	        var result = await _httpClient.GetStringAsync($"{_hostUrl}/api/path/one");

            Assert.That(result, Is.EqualTo(expectedResponse));
	    }
	}
}