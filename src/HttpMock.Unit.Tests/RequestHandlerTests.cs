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

        [Test]
        public void Applies_multiple_constraints()
        {
            var doesNotContainBlahConstraint = new Func<Uri, bool>(uri => uri.AbsoluteUri.Contains("blah") == false);
            var doesNotContainHibriConstraint = new Func<Uri, bool>(uri => uri.AbsoluteUri.Contains("hibri") == false);
            var doesNotContainGoncaloConstraint = new Func<Uri, bool>(uri => uri.AbsoluteUri.Contains("goncalo") == false);

            var h = new RequestHandler("", null);

            h.WithUrlConstraint(doesNotContainBlahConstraint);
            h.WithUrlConstraint(doesNotContainHibriConstraint);
            h.WithUrlConstraint(doesNotContainGoncaloConstraint);

            var uriWithBlah = new Uri("http://www.xyz.com/moomins/goncalo");

            Assert.That(h.CanVerifyConstraintsFor(uriWithBlah), Is.EqualTo(false));
        }
    }
}