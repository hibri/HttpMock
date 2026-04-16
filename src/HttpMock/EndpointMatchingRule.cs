using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace HttpMock
{
	public class EndpointMatchingRule : IMatchingRule
	{
		private static readonly ConcurrentDictionary<string, Regex> PathRegexCache = new();
		private readonly HeaderMatch _headerMatch;
		private readonly QueryParamMatch _queryParamMatch;

		public EndpointMatchingRule()
		{
			_headerMatch = new HeaderMatch();
			_queryParamMatch = new QueryParamMatch();
		}

		public bool IsEndpointMatch(IRequestHandler requestHandler, IHttpRequestHead request) {
			if (requestHandler.QueryParams == null)
				throw new ArgumentException("requestHandler QueryParams cannot be null");

			// Check cheapest conditions first before parsing query/headers
			bool httpMethodsMatch = requestHandler.Method == request.Method;
			if (!httpMethodsMatch) return false;

			bool uriStartsWith = MatchPath(requestHandler, request);
			if (!uriStartsWith) return false;

			bool shouldMatchQueryParams = (requestHandler.QueryParams.Count > 0);
			if (shouldMatchQueryParams) {
				var requestQueryParams = GetQueryParams(request);
				if (!_queryParamMatch.MatchQueryParams(requestHandler, requestQueryParams))
					return false;
			}

			bool shouldMatchHeaders = requestHandler.RequestHeaders != null
				&& requestHandler.RequestHeaders.Count > 0;

			if (shouldMatchHeaders) {
				var requestHeaders = GetHeaders(request);
				if (!_headerMatch.MatchHeaders(requestHandler, requestHeaders))
					return false;
			}

			return true;
		}

	    private static bool MatchPath(IRequestHandler requestHandler, IHttpRequestHead request)
	    {
	        var pathToMatch = request.Uri;
            int positionOfQueryStart = GetStartOfQueryString(request.Uri);
	        if (positionOfQueryStart > -1)
	        {
	            pathToMatch = request.Uri.Substring(0, positionOfQueryStart);
	        }
            var pathMatch = PathRegexCache.GetOrAdd(requestHandler.Path,
                static path => new Regex($@"^{Regex.Escape(path)}\/*$", RegexOptions.Compiled));
	        return pathMatch.IsMatch(pathToMatch);
	    }

	    private static int GetStartOfQueryString(string uri)
	    {
	        return uri.LastIndexOf('?');
	    }

	    private static Dictionary<string, string> GetQueryParams(IHttpRequestHead request) {
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

		private static IDictionary<string, string> GetHeaders(IHttpRequestHead request)
		{
			return request.Headers ?? new Dictionary<string, string>();
		}
	}
}