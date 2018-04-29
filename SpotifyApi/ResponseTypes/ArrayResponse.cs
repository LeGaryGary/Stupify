using System.Collections.Generic;

namespace SpotifyApi.ResponseTypes
{
    public class ListResponse<T> : BasicModel
    {
        public List<T> List { get; set; }
    }
}