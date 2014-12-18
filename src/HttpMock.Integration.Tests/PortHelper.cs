using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Diagnostics;
using System.IO;

namespace HttpMock.Integration.Tests
{
	internal static class PortHelper
	{
		internal static int FindLocalAvailablePortForTesting ()
		{
			const int minPort = 1024;

			var random = new Random ();
			var maxPort = 64000;
			var randomPort = random.Next (minPort, maxPort);


			while (IsPortInUse (randomPort)) {
				randomPort = random.Next (minPort, maxPort);
			}

			return randomPort;
		}

		private static bool IsPortInUse (int randomPort)
		{

			if (Environment.OSVersion.Platform == PlatformID.Unix) {
				var process = new Process () {
					StartInfo = new ProcessStartInfo ("/usr/sbin/lsof", "-Pni") {
						RedirectStandardOutput = true,
						UseShellExecute = false
					}
				};

				using (process) {

					process.Start ();

					var output = process.StandardOutput.ReadToEnd ();

					var lines = output.Split (new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

					return lines.Any (s => s.EndsWith (string.Format ("{0} (LISTEN)", randomPort)));
				}
			

			} else {

				var properties = IPGlobalProperties.GetIPGlobalProperties ();
				return properties.GetActiveTcpConnections ().Any (a => a.LocalEndPoint.Port == randomPort) && properties.GetActiveTcpListeners ().Any (a => a.Port == randomPort);
			}
		}


	}

}