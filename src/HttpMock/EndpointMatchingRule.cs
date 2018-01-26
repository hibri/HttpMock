using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using Kayak.Http;

namespace HttpMock
{
	public class EndpointMatchingRule : IMatchingRule
	{
		private readonly HeaderMatch _headerMatch;
		private readonly QueryParamMatch _queryParamMatch;

		public EndpointMatchingRule()
		{
			_headerMatch = new HeaderMatch();
			_queryParamMatch = new QueryParamMatch();
		}

		public bool IsEndpointMatch(IRequestHandler requestHandler, HttpRequestHead request) {
			if (requestHandler.QueryParams == null)
				throw new ArgumentException("requestHandler QueryParams cannot be null");

			var requestQueryParams = GetQueryParams(request);
			var requestHeaders = GetHeaders(request);

			bool uriStartsWith = MatchPath(requestHandler, request);

			bool httpMethodsMatch = requestHandler.Method == request.Method;
			
			bool queryParamMatch = true;
			bool shouldMatchQueryParams = (requestHandler.QueryParams.Count > 0);
			
			if (shouldMatchQueryParams) {
				queryParamMatch = _queryParamMatch.MatchQueryParams(requestHandler, requestQueryParams);
			}

			bool headerMatch = true;
			bool shouldMatchHeaders = requestHandler.RequestHeaders != null
				&& requestHandler.RequestHeaders.Count > 0;

			if (shouldMatchHeaders) {
				headerMatch = _headerMatch.MatchHeaders(requestHandler, requestHeaders);
			}

			return uriStartsWith && httpMethodsMatch && queryParamMatch && headerMatch;
		}

	    private static bool MatchPath(IRequestHandler requestHandler, HttpRequestHead request)
	    {
	        var pathToMatch = request.Uri;
            int positionOfQueryStart = GetStartOfQueryString(request.Uri);
	        if (positionOfQueryStart > -1)
	        {
	            pathToMatch = request.Uri.Substring(0, positionOfQueryStart);
	        }
            var pathMatch = new Regex(string.Format(@"^{0}\/*$", Regex.Escape(requestHandler.Path)));
	        return pathMatch.IsMatch(pathToMatch);
	    }

	    private static int GetStartOfQueryString(string uri)
	    {
	        return uri.LastIndexOf('?');
	    }

	    private static Dictionary<string, string> GetQueryParams(HttpRequestHead request) {
            int positionOfQueryStart = GetStartOfQueryString(request.Uri);
			if(positionOfQueryStart < 1)
				return new Dictionary<string, string>();

			string queryString = request.Uri.Substring(positionOfQueryStart);
			NameValueCollection valueCollection = HttpUtility.ParseQueryString(queryString);
			var requestQueryParams = valueCollection.AllKeys
				.Where(k => !string.IsNullOrEmpty(k))
				.ToDictionary(k => k, k => valueCollection[k]);
			return requestQueryParams;
		}

		private static IDictionary<string, string> GetHeaders(HttpRequestHead request)
		{
			return request.Headers ?? new Dictionary<string, string>();
		}
	}
}