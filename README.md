![Build status]
(https://ci.appveyor.com/api/projects/status/pqjfme8k5kl7r7av)
(https://ci.appveyor.com/project/hibri/httpmock)

# HttpMock

HttpMock enables you to mock the behaviour of HTTP services, that your application depends on, during testing.
It's particularly useful for Integration and Acceptance testing.

HttpMock returns canned responses at run time.


## Usage.

First, in the application you are testing, change the url of the HTTP service you want to mock, with the url for HttpMock.

Tell HttpMock to listen on the url you've provided. For example:

	_stubHttp = HttpMockRepository.At("http://localhost:9191");

Setup the stub that will return the canned response.

	_stubHttp.Stub(x => x.Get("/endpoint"))
		.Return(expected)
		.OK();

There are three essential parts to setting up a stub.

1. The path that will respond.
	
	stubHttp.Stub(x => x.Get("/endpoint"))

2. The content that will be returned. Supported body types can be Json, file and string content. 

	.Return(expected)

3. The status code of the response.
	
 	.OK()




Eample usage:

	[Test]
 	public void SUT_should_return_stubbed_response() {
		_stubHttp = HttpMockRepository.At("http://localhost:9191");

		const string expected = "<xml><>response>Hello World</response></xml>";
		_stubHttp.Stub(x => x.Get("/endpoint"))
				.Return(expected)
				.OK();

		

		string result = new WebClient().DownloadString("http://localhost:9191/endpoint");

		Console.WriteLine("RESPONSE: {0}", result);

		Assert.That(result, Is.EqualTo(expected));
	}


## Reporting Issues.
When reporting issues, please provide a failing test. 
