.nuget\nuget update -Self
.nuget\nuget restore
.nuget\nuget update HttpMock.sln
msbuild HttpMock.sln /property:Configuration=Release
.nuget\nuget pack HttpMock.nuspec
.nuget\nuget pack HttpMock.Logging.Log4Net.nuspec