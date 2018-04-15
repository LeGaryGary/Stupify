using System.Collections.Generic;

namespace TwitchApi.Responses
{
    public class TwitchResponse<T>
    {
        public IEnumerable<T> Data { get; set; }
    }
}