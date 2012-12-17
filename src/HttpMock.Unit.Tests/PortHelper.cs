using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security;

namespace HttpMock.Unit.Tests
{
	internal static class PortHelper
	{
		internal static int FindLocalAvailablePortForTesting ()
		{
			for (var i = 1025; i <= 65000; i++)
			{
				using (var tcpClient = new TcpClient())
				{
					bool connected = false;
					try
					{
						tcpClient.Connect(IPAddress.Loopback, i);
						connected = tcpClient.Connected;
					}
					catch (SocketException) { }

					if (!connected)
					{
						Console.WriteLine("PortHelper found {0} as available", i);
						return i;
					}
				}
			}
			throw new HostProtectionException("localhost seems to have ALL ports open, are you mad?");
		}
	}
}
