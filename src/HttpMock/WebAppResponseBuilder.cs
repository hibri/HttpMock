namespace HttpMock
{
	public class WebAppResponseBuilder

	{
		private string _body;

		
		

		public WebAppResponseBuilder WithBody(string body) {
			_body = body;
			return this;
		}
		public  string Build() {
			return _body;
		}
	}
}