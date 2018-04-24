using System;
using System.Runtime.Serialization;

namespace SpotifyApi.ResponseTypes
{
    [Flags]
    public enum AlbumType
    {
        [EnumMember(Value = "album")] 
        Album = 1,

        [EnumMember(Value = "single")] 
        Single = 2,

        [EnumMember(Value = "compilation")] 
        Compilation = 4,

        [EnumMember(Value = "appears_on")] 
        AppearsOn = 8,

        [EnumMember(Value = "album,single,compilation,appears_on")] 
        All = 16
    }
}