using System;
using System.Runtime.Serialization;

namespace SpotifyApi.ResponseTypes
{
    [Flags]
    public enum SearchType
    {
        [EnumMember(Value = "artist")] 
        Artist = 1,

        [EnumMember(Value = "album")] 
        Album = 2,

        [EnumMember(Value = "track")] 
        Track = 4,

        [EnumMember(Value = "playlist")] 
        Playlist = 8,

        [EnumMember(Value = "track,album,artist,playlist")] 
        All = 16
    }
}