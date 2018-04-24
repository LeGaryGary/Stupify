using System;
using System.Runtime.Serialization;

namespace SpotifyApi.ResponseTypes
{
    [Flags]
    public enum RepeatState
    {
        [EnumMember(Value = "track")] 
        Track = 1,

        [EnumMember(Value = "context")]
        Context = 2,

        [EnumMember(Value = "off")] 
        Off = 4
    }
}