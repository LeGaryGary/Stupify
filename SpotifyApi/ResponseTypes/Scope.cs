using System;
using System.Runtime.Serialization;

namespace SpotifyApi.ResponseTypes
{
    [Flags]
    public enum Scope
    {
        [EnumMember(Value = "")]
        None = 1,

        [EnumMember(Value = "playlist-modify-public")] 
        PlaylistModifyPublic = 2,

        [EnumMember(Value = "playlist-modify-private")] 
        PlaylistModifyPrivate = 4,

        [EnumMember(Value = "playlist-read-private")] 
        PlaylistReadPrivate = 8,

        [EnumMember(Value = "streaming")] 
        Streaming = 16,

        [EnumMember(Value = "user-read-private")] 
        UserReadPrivate = 32,

        [EnumMember(Value = "user-read-email")] 
        UserReadEmail = 64,

        [EnumMember(Value = "user-library-read")] 
        UserLibraryRead = 128,

        [EnumMember(Value = "user-library-modify")] 
        UserLibraryModify = 256,

        [EnumMember(Value = "user-follow-modify")] 
        UserFollowModify = 512,

        [EnumMember(Value = "user-follow-read")] 
        UserFollowRead = 1024,

        [EnumMember(Value = "user-read-birthdate")] 
        UserReadBirthdate = 2048,

        [EnumMember(Value = "user-top-read")] 
        UserTopRead = 4096,

        [EnumMember(Value = "playlist-read-collaborative")] 
        PlaylistReadCollaborative = 8192,

        [EnumMember(Value = "user-read-recently-played")] 
        UserReadRecentlyPlayed = 16384,

        [EnumMember(Value = "user-read-playback-state")] 
        UserReadPlaybackState = 32768,

        [EnumMember(Value = "user-modify-playback-state")] 
        UserModifyPlaybackState = 65536
    }
}