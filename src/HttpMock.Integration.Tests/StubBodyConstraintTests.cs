using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
    [TestFixture]
    public class StubBodyConstraintTests
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private string _hostUrl;
        private IHttpServer _stubHttp;

        [SetUp]
        public void SetUp()
        {
            _hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
            _stubHttp = HttpMockRepository.At(_hostUrl);
        }

        [Test]
        public async Task Stub_with_exact_body_match_returns_correct_response()
        {
            _stubHttp
                .Stub(x => x.Post("/api"))
                .WithBody("hello")
                .Return("matched hello")
                .OK();

            var response = await _httpClient.PostAsync(
                $"{_hostUrl}/api",
                new StringContent("hello", Encoding.UTF8));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var body = await response.Content.ReadAsStringAsync();
            Assert.That(body, Is.EqualTo("matched hello"));
        }

        [Test]
        public async Task Stub_with_exact_body_match_returns_404_when_body_does_not_match()
        {
            _stubHttp
                .Stub(x => x.Post("/api"))
                .WithBody("hello")
                .Return("matched hello")
                .OK();

            var response = await _httpClient.PostAsync(
                $"{_hostUrl}/api",
                new StringContent("world", Encoding.UTF8));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Two_stubs_on_same_path_disambiguated_by_body()
        {
            _stubHttp
                .Stub(x => x.Post("/api"))
                .WithBody("first")
                .Return("response for first")
                .OK();

            _stubHttp
                .Stub(x => x.Post("/api"))
                .WithBody("second")
                .Return("response for second")
                .OK();

            var firstResponse = await _httpClient.PostAsync(
                $"{_hostUrl}/api",
                new StringContent("first", Encoding.UTF8));
            var firstBody = await firstResponse.Content.ReadAsStringAsync();

            var secondResponse = await _httpClient.PostAsync(
                $"{_hostUrl}/api",
                new StringContent("second", Encoding.UTF8));
            var secondBody = await secondResponse.Content.ReadAsStringAsync();

            Assert.That(firstBody, Is.EqualTo("response for first"));
            Assert.That(secondBody, Is.EqualTo("response for second"));
        }

        [Test]
        public async Task Stub_with_predicate_body_match_returns_correct_response()
        {
            _stubHttp
                .Stub(x => x.Post("/api"))
                .WithBody(body => body != null && body.Contains("important"))
                .Return("matched predicate")
                .OK();

            var response = await _httpClient.PostAsync(
                $"{_hostUrl}/api",
                new StringContent("this is important data", Encoding.UTF8));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.That(responseBody, Is.EqualTo("matched predicate"));
        }

        [Test]
        public async Task Stub_with_predicate_body_match_returns_404_when_predicate_fails()
        {
            _stubHttp
                .Stub(x => x.Post("/api"))
                .WithBody(body => body != null && body.Contains("important"))
                .Return("matched predicate")
                .OK();

            var response = await _httpClient.PostAsync(
                $"{_hostUrl}/api",
                new StringContent("irrelevant data", Encoding.UTF8));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task Stub_without_body_constraint_matches_any_body()
        {
            _stubHttp
                .Stub(x => x.Post("/api"))
                .Return("no constraint")
                .OK();

            var response = await _httpClient.PostAsync(
                $"{_hostUrl}/api",
                new StringContent("anything at all", Encoding.UTF8));

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseBody = await response.Content.ReadAsStringAsync();
            Assert.That(responseBody, Is.EqualTo("no constraint"));
        }

        [Test]
        public async Task At_with_logger_factory_starts_server_and_returns_responses()
        {
            var hostUrl = HostHelper.GenerateAHostUrlForAStubServer();
            var server = HttpMockRepository.At(hostUrl, Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);

            server.Stub(x => x.Get("/ping"))
                .Return("pong")
                .OK();

            var result = await _httpClient.GetStringAsync($"{hostUrl}/ping");

            Assert.That(result, Is.EqualTo("pong"));
        }
    }
}
