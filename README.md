[![Build status]
(https://ci.appveyor.com/api/projects/status/github/stevenao/httpMock)]
(https://ci.appveyor.com/project/stevenao/httpmock)

A .Net library to create HTTP servers, which returns canned responses at run time.

Like a stubbing library, but with actual HTTP Responses.

Eample usage:

	[Test]
 	public void SUT_should_return_stubbed_response() {
		_stubHttp = HttpMockRepository.At("http://localhost:9191");

		const string expected = "<xml>response>Hello World</response></xml>";
		_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

		

		string result = new WebClient().DownloadString("http://localhost:9191/endpoint");

		Console.WriteLine("RESPONSE: {0}", result);

		Assert.That(result, Is.EqualTo(expected));
	}

I have removed dependecy on NUnit in the HttpMock project. The hosting application will have to implement the IHttpMockAssert interface.