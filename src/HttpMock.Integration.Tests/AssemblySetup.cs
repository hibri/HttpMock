using NUnit.Framework;
using log4net.Config;

namespace SevenDigital.HttpMock.Integration.Tests
{
	[SetUpFixture]
	public class AssemblySetup
	{
		public AssemblySetup() {
			XmlConfigurator.Configure();
		}
	}
}