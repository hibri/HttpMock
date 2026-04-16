using System.Diagnostics;
using System.Reflection;

namespace HttpMock
{
	/// <summary>
	/// Provides the <see cref="System.Diagnostics.ActivitySource"/> used by HttpMock for
	/// OpenTelemetry-compatible distributed tracing.
	/// <para>
	/// To capture HttpMock traces in your test output, subscribe to this source via
	/// <c>tracerProviderBuilder.AddSource(HttpMockActivitySource.Name)</c> when configuring
	/// the OpenTelemetry SDK.
	/// </para>
	/// </summary>
	public static class HttpMockActivitySource
	{
		/// <summary>The name of the <see cref="ActivitySource"/> used by HttpMock.</summary>
		public const string Name = "HttpMock";

		private static readonly string Version =
			typeof(HttpMockActivitySource).Assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				?.InformationalVersion ?? "0.0.0";

		/// <summary>The shared <see cref="ActivitySource"/> instance.</summary>
		public static readonly ActivitySource Source = new ActivitySource(Name, Version);
	}
}
