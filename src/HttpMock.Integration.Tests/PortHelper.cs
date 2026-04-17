namespace HttpMock.Integration.Tests
{
internal static class PortHelper
{
internal static int FindLocalAvailablePortForTesting()
{
return HttpMock.HttpMockRepository.FindFreePort();
}
}
}
