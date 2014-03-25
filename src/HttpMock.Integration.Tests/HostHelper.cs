using System;

namespace HttpMock.Integration.Tests
{
	internal static class HostHelper
	{
		public static string GenerateAHostUrlForAStubServerWith(string basePath)
		{
			return String.Format("{0}/{1}", GenerateAHostUrlForAStubServer(), basePath);
		}


		public static string GenerateAHostUrlForAStubServer()
		{
			return String.Format("http://localhost:{0}", PortHelper.FindLocalAvailablePortForTesting());
		}
	}
}