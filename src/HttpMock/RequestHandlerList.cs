using System.Collections.Generic;

namespace HttpMock
{
    public interface IRequestHandlerList : IList<IRequestHandler>
    {
        
    }

    public class RequestHandlerList : List<IRequestHandler>, IRequestHandlerList
    {
		
	}
}