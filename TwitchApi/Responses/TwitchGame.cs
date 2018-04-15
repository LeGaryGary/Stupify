using Newtonsoft.Json;

namespace TwitchApi.Responses
{
    public class TwitchGame
    {
        [JsonProperty("box_art_url")] public string BoxArtUrl { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("name")] public string Name { get; set; }
    }
}