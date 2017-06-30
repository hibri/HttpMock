using HttpMock.Logging.Log4Net;
using log4net.Config;
using NUnit.Framework;

namespace HttpMock.Integration.Tests
{
	[SetUpFixture]
	public class AssemblySetup
	{
		public AssemblySetup() {
			XmlConfigurator.Configure();
			Log4NetFactory.UseLog4Net();
		}
	}
}