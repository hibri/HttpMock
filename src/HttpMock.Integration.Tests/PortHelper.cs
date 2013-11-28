using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security;

namespace HttpMock.Integration.Tests
{
	internal static class PortHelper
	{
		internal static int FindLocalAvailablePortForTesting ()
		{
			for (var i = 1025; i <= 65000; i++)
			{
				if (!ConnectToPort(i))
					return i;
			}
			throw new HostProtectionException("localhost seems to have ALL ports open, are you mad?");
		}

		private static bool ConnectToPort(int i)
		{
			var allIpAddresses = (from adapter in NetworkInterface.GetAllNetworkInterfaces()
			                      from unicastAddress in adapter.GetIPProperties().UnicastAddresses
			                      select unicastAddress.Address)
				.ToList();

			bool connected = false;
			foreach (var ipAddress in allIpAddresses)
			{
				using (var tcpClient = new TcpClient())
				{
					try
					{
						tcpClient.Connect(ipAddress, i);
						connected = tcpClient.Connected;
					}
					catch (SocketException)
					{
					}
					finally
					{
						try
						{
							tcpClient.Close();
						}
						catch
						{
						}
					}
				}
				if (connected)
					return true;
			}
			return false;
		}
	}
}
