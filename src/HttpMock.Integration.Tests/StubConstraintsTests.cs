using System;
using System.Net;
using HttpMock;
using NUnit.Framework;

namespace SevenDigital.HttpMock.Integration.Tests
{
    public class StubConstraintsTests
    {
        private IHttpServer _httpMockRepository;
        private WebClient _wc;
        private IHttpServer _stubHttp;

        [SetUp]
        public void SetUp()
        {
            _httpMockRepository = HttpMockRepository.At("http://localhost:8080");
            _wc = new WebClient();
            _stubHttp = _httpMockRepository.WithNewContext();
        }

        [Test]
        public void Constraints_can_be_applied_to_urls()
        {
            _stubHttp
                .Stub(x => x.Post("/firsttest"))
                .WithUrlConstraint(url => url.Contains("/blah/blah") == false)
                .Return("<Xml>ShouldntBeReturned</Xml>")
                .OK();

            try
            {
                _wc.UploadString("Http://localhost:8080/firsttest/blah/blah", "x");

                Assert.Fail("Should have 404d");
            }
            catch (WebException ex)
            {
                Assert.That(ex.Message, Is.EqualTo("The remote server returned an error: (404) Not Found."));
            } 
        }
    }
}