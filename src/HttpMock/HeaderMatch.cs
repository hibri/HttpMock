using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpMock
{
	public class HeaderMatch {
		internal bool MatchHeaders(IRequestHandler requestHandler, IDictionary<string, string> requestHeaders)
		{
			return requestHandler.RequestHeaders.All(
				expectedHeader => requestHeaders.Any(header => HeadersMatch(expectedHeader, header)));
		}

		private static bool HeadersMatch(KeyValuePair<string, string> expectedHeader, KeyValuePair<string, string> header)
		{
			if (!string.Equals(expectedHeader.Key, header.Key, StringComparison.OrdinalIgnoreCase))
			{
				return false;
			}
			return string.Equals(expectedHeader.Value, header.Value, StringComparison.OrdinalIgnoreCase);
		}
	}
}