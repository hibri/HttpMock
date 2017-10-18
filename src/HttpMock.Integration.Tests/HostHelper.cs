using System;

namespace HttpMock.Integration.Tests
{
	internal static class HostHelper
	{
        private static object lckObject=new object();
         
		public static string GenerateAHostUrlForAStubServerWith(string basePath)
		{
			return String.Format("{0}/{1}", GenerateAHostUrlForAStubServer(), basePath);
		}


		public static string GenerateAHostUrlForAStubServer()
		{
		    lock (lckObject)
		    {
		        return String.Format("http://localhost:{0}", PortHelper.FindLocalAvailablePortForTesting());
		    }
		}
	}
}