using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class HttpMockRepositoryTests
	{
		[Test]
		public void Should_create_a_http_server() {
			
			Assert.That(HttpMockRepository.At("http://something/something"), Is.Not.Null);
		}
	}
}
