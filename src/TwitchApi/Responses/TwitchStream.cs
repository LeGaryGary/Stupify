using Newtonsoft.Json;

namespace TwitchApi.Responses
{
    public class TwitchStream
    {
        [JsonProperty("community_ids")] public string[] CommunityIds { get; set; }
        [JsonProperty("game_id")] public string GameId { get; set; }
        [JsonProperty("id")] public string Id { get; set; }
        [JsonProperty("language")] public string Language { get; set; }
        [JsonProperty("started_at")] public string StartedAtUtc { get; set; }
        [JsonProperty("thumbnail_url")] public string ThumbnailUrl { get; set; }
        [JsonProperty("title")] public string Title { get; set; }
        [JsonProperty("type")] public string StreamType { get; set; }
        [JsonProperty("user_id")] public string UserId { get; set; }
        [JsonProperty("viewer_count")] public int ViewerCount { get; set; }
    }
}