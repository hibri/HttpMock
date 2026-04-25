using System.Collections.Generic;

namespace HttpMock
{
    public interface IRequestMatcher
    {
        IRequestHandler Match(IHttpRequestHead request, IEnumerable<IRequestHandler> requestHandlerList, string body = null);
    }

    public class RequestMatcher : IRequestMatcher
    {
        private readonly IMatchingRule _matchingRule;

        public RequestMatcher(IMatchingRule matchingRule)
        {
            _matchingRule = matchingRule;
        }

        public IRequestHandler Match(IHttpRequestHead request, IEnumerable<IRequestHandler> requestHandlerList, string body = null)
        {
            foreach (var handler in requestHandlerList)
            {
                if (_matchingRule.IsEndpointMatch(handler, request)
                    && handler.CanVerifyConstraintsFor(request.Uri)
                    && handler.MatchesBody(body))
                {
                    return handler;
                }
            }

            return null;
        }
    }
}