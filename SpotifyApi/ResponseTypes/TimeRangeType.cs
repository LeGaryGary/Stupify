using System;
using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    /// <summary>
    ///     Only one value allowed
    /// </summary>
    [Flags]
    public enum TimeRangeType
    {
        [JsonProperty("long_term")]
        LongTerm = 1,

        [JsonProperty("medium_term")]
        MediumTerm = 2,

        [JsonProperty("short_term")]
        ShortTerm = 4
    }
}