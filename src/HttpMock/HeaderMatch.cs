using System;
using System.Collections.Generic;
using System.Linq;

namespace HttpMock
{
	public class HeaderMatch {
		internal bool MatchHeaders(IRequestHandler requestHandler, IDictionary<string, string> requestHeaders)
		{
			foreach (var expectedHeader in requestHandler.RequestHeaders)
			{
				if (!requestHeaders.TryGetValue(expectedHeader.Key, out var actualValue))
				{
					// Try case-insensitive key lookup as a fallback
					bool found = false;
					foreach (var header in requestHeaders)
					{
						if (string.Equals(header.Key, expectedHeader.Key, StringComparison.OrdinalIgnoreCase)
						    && string.Equals(header.Value, expectedHeader.Value, StringComparison.OrdinalIgnoreCase))
						{
							found = true;
							break;
						}
					}
					if (!found) return false;
				}
				else if (!string.Equals(actualValue, expectedHeader.Value, StringComparison.OrdinalIgnoreCase))
				{
					return false;
				}
			}
			return true;
		}
	}
}