using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class PathMatchingTests
	{
		[Test]
		public void Should_match() {
			Assert.That(PathMatch("/app/path/endpoint", "/path/endpoint"), Is.True);
			Assert.That(PathMatch("/app/path/endpoint", "/endpoint"), Is.False);
		}

		private bool PathMatch(string path, string requestUri) {

			return path.StartsWith(requestUri);
		}
	}
}
