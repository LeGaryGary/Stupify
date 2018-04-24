﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Newtonsoft.Json;

namespace SpotifyApi.ResponseTypes
{
    public class TuneableTrack
    {
        [JsonProperty("acousticness")]
        public float? Acousticness { get; set; }

        [JsonProperty("danceability")]
        public float? Danceability { get; set; }

        [JsonProperty("duration_ms")]
        public int? DurationMs { get; set; }

        [JsonProperty("energy")]
        public float? Energy { get; set; }

        [JsonProperty("instrumentalness")]
        public float? Instrumentalness { get; set; }

        [JsonProperty("key")]
        public int? Key { get; set; }

        [JsonProperty("liveness")]
        public float? Liveness { get; set; }

        [JsonProperty("loudness")]
        public float? Loudness { get; set; }

        [JsonProperty("mode")]
        public int? Mode { get; set; }

        [JsonProperty("popularity")]
        public int? Popularity { get; set; }

        [JsonProperty("speechiness")]
        public float? Speechiness { get; set; }

        [JsonProperty("tempo")]
        public float? Tempo { get; set; }

        [JsonProperty("time_signature")]
        public int? TimeSignature { get; set; }

        [JsonProperty("valence")]
        public float? Valence { get; set; }

        //public string BuildUrlParams(string prefix)
        //{
        //    List<string> urlParams = new List<string>();
        //    foreach (PropertyInfo info in GetType().GetProperties())
        //    {
        //        object value = info.GetValue(this);
        //        string name = info.GetCustomAttribute<StringAttribute>()?.Text;
        //        if(name == null || value == null)
        //            continue;
        //        if (value is float)
        //            urlParams.Add($"{prefix}_{name}={((float)value).ToJsonProperty(CultureInfo.InvariantCulture)}");
        //        else
        //            urlParams.Add($"{prefix}_{name}={value}");
        //    }
        //    if (urlParams.Count > 0)
        //        return "&" + string.Join("&", urlParams);
        //    return "";
        //}
    }
}