using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Kayak.Http;

namespace HttpMock
{
	public class EndpointMatchingRule : IMatchingRule
	{
		public bool IsEndpointMatch(IRequestHandler requestHandler, HttpRequestHead request) {
			if (requestHandler.QueryParams == null)
				throw new ArgumentException("requestHandler QueryParams cannot be null");

			var requestQueryParams = GetQueryParams(request);

			bool uriStartsWith = request.Uri.StartsWith(requestHandler.Path);

			bool httpMethodsMatch = requestHandler.Method == request.Method;
			
			bool parametersExist = true;

			foreach (var queryParam in requestHandler.QueryParams) {
				if(!requestQueryParams.ContainsKey(queryParam.Key)) {
					parametersExist = false;
					break;
				}
				if (requestQueryParams[queryParam.Key] != queryParam.Value) {
					parametersExist = false;
					break;
				}
			}
			
			return uriStartsWith && httpMethodsMatch && parametersExist;
		}

		private static Dictionary<string, string> GetQueryParams(HttpRequestHead request) {
			int pos = request.Uri.LastIndexOf('?');
			if(pos < 1)
				return new Dictionary<string, string>();

			string queryString = request.Uri.Substring(pos);
			NameValueCollection valueCollection = HttpUtility.ParseQueryString(queryString);
			var requestQueryParams = valueCollection.AllKeys.ToDictionary(k => k, k => valueCollection[k]);
			return requestQueryParams;
		}
	}
}