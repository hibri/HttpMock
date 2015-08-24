using System;
using System.Diagnostics;
using System.Linq;

namespace HttpMock {
	internal static class HttpMockAssert {
		static private readonly Lazy<IHttpMockAssert> CurrentAssertion = new Lazy<IHttpMockAssert>( ResolveAssertion );

		private static IHttpMockAssert ResolveAssertion() {
			var type = typeof( IHttpMockAssert );
			var targetType = AppDomain.CurrentDomain.GetAssemblies().SelectMany( a => a.GetTypes() ).FirstOrDefault( p => type.IsAssignableFrom( p ) && !p.IsInterface );
			if( targetType == null ) throw new NotImplementedException( "Missing HttpMock.IHttpMockAssert implementation class" );
			
			Trace.WriteLine( string.Format( "HttpMock is using implementation {0}", targetType.FullName ) );
			return (IHttpMockAssert)Activator.CreateInstance( targetType );
		}

		private static IHttpMockAssert Current { get { return CurrentAssertion.Value; } }

		public static void IsNotNull( object actual, string message ) {
			Current.IsNotNull( actual, message );
		}

		public static void IsNull( object actual, string message ) {
			Current.IsNull( actual, message );
		}

		public static void IsGreaterThan<T>( T actual, T value, string message ) {
			Current.IsGreaterThan( actual, value, message );
		}

		public static void IsEqual<T>( T actual, T value, string message ) {
			Current.IsEqual( actual, value, message );
		}
	}
}