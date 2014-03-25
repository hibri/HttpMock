using System;
using System.Linq;
using System.Net.NetworkInformation;

namespace HttpMock.Integration.Tests
{
	internal static class PortHelper
	{
		internal static int FindLocalAvailablePortForTesting ()
		{
			IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
		    var activeTcpConnections = properties.GetActiveTcpConnections();
		    const int minPort = 1024;
            
		    var random = new Random();
		    var maxPort = 64000;
		    var randomPort = random.Next(minPort, maxPort);


            while (activeTcpConnections.Any(a => a.LocalEndPoint.Port == randomPort))
		    {
                randomPort = random.Next(minPort, maxPort);
		    }
		    return randomPort;
		}
	}
}
