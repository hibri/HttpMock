using System;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
    [TestFixture]
    public class RequestHandlerTests
    {
        [Test]
        public void Applies_One_Constraint()
        {
            var doesNotContainBlahConstraint = new Func<Uri, bool>(uri => uri.AbsoluteUri.Contains("blah") == false);

            var h = new RequestHandler("", null);

            h.WithUrlConstraint(doesNotContainBlahConstraint);

            var uriWithBlah = new Uri("http://www.blah.com");

            Assert.That(h.CanVerifyConstraintsFor(uriWithBlah), Is.EqualTo(false));
        }

        // applies multiple constraints 
    }
}