using System;
using NUnit.Framework;
using HttpMock;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class HttpMockRepositoryTests
	{
		[Test]
		public void DoNotAcceptTrailingSlash ()
		{
			Assert.Throws<ArgumentException> ( () => HttpMockRepository.At("http://localhost:9900/"));
			Assert.DoesNotThrow ( () => HttpMockRepository.At("http://localhost:9900"));
		}
	}
}

