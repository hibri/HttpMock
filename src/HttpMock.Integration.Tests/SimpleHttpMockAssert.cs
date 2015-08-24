using NUnit.Framework;

namespace HttpMock.Integration.Tests {
	public class SimpleHttpMockAssert : IHttpMockAssert {

		public void IsNotNull( object actual, string message ) {
			Assert.That( actual, Is.Not.Null, message );
		}

		public void IsNull( object actual, string message ) {
			Assert.That( actual, Is.Null, message );
		}

		public void IsGreaterThan<T>( T actual, T value, string message ) {
			Assert.That( actual, Is.GreaterThan( value ), message );
		}

		public void IsEqual<T>( T actual, T value, string message ) {
			Assert.That( actual, Is.EqualTo( value ), message );
		}
	}
}