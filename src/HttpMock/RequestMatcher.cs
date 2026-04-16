using System.Collections.Generic;
using System.Linq;

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
            var matches = requestHandlerList
                .Where(handler => _matchingRule.IsEndpointMatch(handler, request))
                .Where(handler => handler.CanVerifyConstraintsFor(request.Uri))
                .Where(handler => handler.MatchesBody(body));

            return matches.FirstOrDefault();
        }
    }
}