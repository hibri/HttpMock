using System;
using FakeItEasy;
using NUnit.Framework;

namespace HttpMock.Unit.Tests
{
	[TestFixture]
	public class HttpServerFactoryTests
	{
		private IHttpServerBuilder _fakeBuilder;
		private IHttpServer _fakeServer;

		[SetUp]
		public void SetUp() {
			_fakeBuilder = A.Fake<IHttpServerBuilder>();
			_fakeServer = A.Fake<IHttpServer>();
		}

		[Test]
		public void Should_create_a_http_server() {
			Uri uri = new Uri("http://blah/blah");

			A.CallTo(() => _fakeBuilder.Build(uri)).Returns(_fakeServer);

			var httpServer = new HttpServerFactory(_fakeBuilder).Create(uri);

			Assert.That(httpServer, Is.EqualTo(_fakeServer));
		}

		[Test]
		public void Should_start_the_server() {
			Uri uri = new Uri("http://blah/blah");
			A.CallTo(() => _fakeBuilder.Build(uri)).Returns(_fakeServer);

			new HttpServerFactory(_fakeBuilder).Create(uri);
			A.CallTo( () => _fakeServer.Start()).MustHaveHappened();
		}

		[Test]
		public void Should_return_the_same_server_when_already_created_for_the_same_uri() {

			Uri uri = new Uri("http://blah/blah");

			A.CallTo(() => _fakeBuilder.Build(uri)).Returns(_fakeServer);

			HttpServerFactory httpServerFactory = new HttpServerFactory(_fakeBuilder);
			var httpServer1 = httpServerFactory.Create(uri);
			var httpServer2 = httpServerFactory.Create(uri);

			Assert.That(httpServer1, Is.EqualTo(httpServer2));
		}

		[Test]
		public void Should_not_start_the_server_if_already_started_for_same_uri()
		{

			Uri uri = new Uri("http://blah/blah");

			A.CallTo(() => _fakeBuilder.Build(uri)).Returns(_fakeServer);

			HttpServerFactory httpServerFactory = new HttpServerFactory(_fakeBuilder);
			var httpServer1 = httpServerFactory.Create(uri);
			var httpServer2 = httpServerFactory.Create(uri);

			A.CallTo(() => _fakeServer.Start()).MustHaveHappened(Repeated.Exactly.Once);

		}
	}
}
