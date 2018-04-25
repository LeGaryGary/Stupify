using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    public class AnalysisTimeSlice
    {
        [JsonProperty("start")]
        public double Start { get; set; }

        [JsonProperty("duration")]
        public double Duration { get; set; }

        [JsonProperty("confidence")]
        public double Confidence { get; set; }
    }
}
