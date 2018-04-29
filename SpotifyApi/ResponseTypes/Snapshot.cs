using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    public class Snapshot : BasicModel
    {
        [JsonProperty("snapshot_id")]
        public string SnapshotId { get; set; }
    }
}