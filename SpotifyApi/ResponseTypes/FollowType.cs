using System;
using System.Runtime.Serialization;

namespace SpotifyApi.ResponseTypes
{
    [Flags]
    public enum FollowType
    {
        [EnumMember(Value = "artist")] 
        Artist = 1,

        [EnumMember(Value = "user")] 
        User = 2
    }
}