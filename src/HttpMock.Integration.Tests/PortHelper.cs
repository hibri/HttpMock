using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;

namespace HttpMock.Integration.Tests
{
	internal static class PortHelper
	{
		internal static int FindLocalAvailablePortForTesting()
		{
			
			const int minPort = 1024;

			var random = new Random();
			var maxPort = 64000;
			var randomPort = random.Next(minPort, maxPort);


			while (IsPortInUse(randomPort))
			{
				randomPort = random.Next(minPort, maxPort);
			}
			return randomPort;
		}

		private static bool IsPortInUse(int randomPort)
		{
			var properties = IPGlobalProperties.GetIPGlobalProperties();
			return properties.GetActiveTcpConnections().Any(a => a.LocalEndPoint.Port == randomPort) && properties.GetActiveTcpListeners().Any( a=> a.Port == randomPort);
		}
	}
}