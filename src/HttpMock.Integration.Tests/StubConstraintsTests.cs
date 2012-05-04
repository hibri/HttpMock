using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
    public class StubConstraintsTests
    {
        private IHttpServer _httpMockRepository;

        [SetUp]
        public void SetUp()
        {
            _httpMockRepository = HttpMockRepository.At("http://localhost:8080/");
        }

        [Test]
        public void FirstTest()
        {
            var wc = new WebClient();
            string stubbedReponse = "<Xml>ShouldntBeReturned</Xml>";
           
            var stubHttp = _httpMockRepository
                .WithNewContext();

            stubHttp
                .Stub(x => x.Post("/firsttest"))
                .WithUrlConstraint(url => url.AbsoluteUri.Contains("/blah/blah") == false)
                .Return(stubbedReponse)
                .OK();

            Assert.That(wc.UploadString("Http://localhost:8080/firsttest/blah/blah", "x"), Is.Not.EqualTo(stubbedReponse));
        }
    }
}