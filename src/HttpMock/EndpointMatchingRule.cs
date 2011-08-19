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
			
			bool queryParamMatch = true;
			bool shouldMatchQueryParams = (requestHandler.QueryParams.Count > 0);
			
			if (shouldMatchQueryParams) {
				queryParamMatch = MatchQueryParams(requestHandler, requestQueryParams);
			}

			return uriStartsWith && httpMethodsMatch && queryParamMatch;
		}

		private static bool MatchQueryParams(IRequestHandler requestHandler, Dictionary<string, string> requestQueryParams) {
			foreach (var queryParam in requestHandler.QueryParams) {
				if (!requestQueryParams.ContainsKey(queryParam.Key)) {
					return false;
				}
				if (requestQueryParams[queryParam.Key] != queryParam.Value) {
					return false;
				}
			}
			return true;
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