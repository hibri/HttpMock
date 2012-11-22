﻿using System;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class HttpServerTests
	{
		[Test]
		public void IsAvailableReturnsFalseIfStartNotCalled()
		{
			IHttpServer httpServer = new HttpServer(new Uri("http://localhost:9099"));
			Assert.That(httpServer.IsAvailable(), Is.EqualTo(false));
		}
	}
}