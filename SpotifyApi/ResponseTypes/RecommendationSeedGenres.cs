using System.Collections.Generic;
using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    public class RecommendationSeedGenres : BasicModel
    {
         [JsonProperty("genres")]
         public List<string> Genres { get; set; }
    }
}