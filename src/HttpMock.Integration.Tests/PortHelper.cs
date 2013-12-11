using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security;

namespace HttpMock.Integration.Tests
{
	internal static class PortHelper
	{
		internal static int FindLocalAvailablePortForTesting ()
		{
			IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
		    var activeTcpConnections = properties.GetActiveTcpConnections();
		    var minPort = activeTcpConnections.Select(a => a.LocalEndPoint.Port).Max();
            
		    var random = new Random();
		    var randomPort = random.Next(minPort, 65000);


            while (activeTcpConnections.Any(a => a.LocalEndPoint.Port == randomPort))
		    {
                randomPort = random.Next(minPort, 65000);
		    }
		    return randomPort;
		}
	}
}
