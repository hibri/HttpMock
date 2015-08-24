namespace HttpMock {
	public interface IHttpMockAssert {
		void IsNotNull( object actual, string message );
		void IsNull( object actual, string message );
		void IsGreaterThan<T>( T actual, T value, string message );
		void IsEqual<T>( T actual, T value, string message );
	}
}