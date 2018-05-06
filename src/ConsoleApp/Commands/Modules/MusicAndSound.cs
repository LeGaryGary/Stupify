using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Discord;
using Discord.Commands;
using NYoutubeDL;
using Google.Apis.YouTube.v3;
using SpotifyApi;
using Stupify.Data.Models;
using Stupify.Data.Repositories;
using StupifyConsoleApp.Client.Audio;

namespace StupifyConsoleApp.Commands.Modules
{
    public class MusicAndSound : ModuleBase<CommandContext>
    {
        private const string LinkSpotify = "Please link your spotify account to do this! http://stupifybot.com";
        private readonly AudioService _audioService;
        private readonly YoutubeDL _youtubeDl;
        private readonly YouTubeService _youTubeService;
        private readonly MusicSearches _musicSearches;
        private readonly IExternalAccountRepository _externalAccountRepository;

        public MusicAndSound(AudioService audioService, YoutubeDL youtubeDl, YouTubeService youTubeService, MusicSearches musicSearches, IExternalAccountRepository externalAccountRepository)
        {
            _audioService = audioService;
            _youtubeDl = youtubeDl;
            _youTubeService = youTubeService;
            _musicSearches = musicSearches;
            _externalAccountRepository = externalAccountRepository;
        }

        private SpotifyClient _spotifyClient;
        private async Task<SpotifyClient> GetSpotifyClientAsync()
        {
            if (_spotifyClient != null) return _spotifyClient;
            
            var auth = await _externalAccountRepository.GetAsync(Context.User.Id, ExternalService.Spotify).ConfigureAwait(false);
            if (auth == null) return null;

            _spotifyClient = new SpotifyClient(auth.AccessToken);

            return _spotifyClient;
        }

        [Command("AirHorn", RunMode = RunMode.Async)]
        public async Task PlayAirHornAsync()
        {
            if (Context.User is IVoiceState state && state.VoiceChannel != null)
                await _audioService.QueueFileAsync(Context.Channel as ITextChannel, state.VoiceChannel, Directory.GetCurrentDirectory() + "\\AirHorn.mp3").ConfigureAwait(false);
        }

        [Command("play", RunMode = RunMode.Async), Priority(1)]
        public async Task DownloadAndPlayFromYoutubeAsync(Uri requestUri)
        {
            if (!(Context.User is IGuildUser user) || user.VoiceChannel == null) return;

            var uriString = requestUri.AbsoluteUri;

            var spotifyTrackIndex = -1;
            var spotifyUserIndex = -1;
            var spotifyPlaylistIndex = -1;
            var spotifyAlbumIndex = -1;

            if (uriString.Contains("open.spotify.com"))
            {
                spotifyTrackIndex = uriString.IndexOf("track", StringComparison.OrdinalIgnoreCase);
                spotifyPlaylistIndex = uriString.IndexOf("playlist", StringComparison.OrdinalIgnoreCase);
                spotifyUserIndex = uriString.IndexOf("user", StringComparison.OrdinalIgnoreCase);
                spotifyAlbumIndex = uriString.IndexOf("album", StringComparison.OrdinalIgnoreCase);
            }

            var query = HttpUtility.ParseQueryString(requestUri.Query);

            var listValues = query.GetValues("list");
            var vValues = query.GetValues("v");

            // Handle youtube Playlist
            if (listValues != null)
            {
                var playlistId = listValues.First();
                var ids = await GetVideoIdsAsync(playlistId).ConfigureAwait(false);
                await VerifyAndQueueAsync(ids).ConfigureAwait(false);
            }

            // Handle youtube Video
            else if (vValues != null)
            {
                var id = vValues.First();
                

                await VerifyAndQueueAsync(id).ConfigureAwait(false);
            }

            // Handle Spotify Playlist
            else if (spotifyPlaylistIndex > 0 && spotifyUserIndex > 0)
            {
                var startIndex = spotifyUserIndex + "user/".Length;
                var userId = uriString.Substring(startIndex, spotifyPlaylistIndex - startIndex - 1);
                var playlistId = uriString.Substring(spotifyPlaylistIndex + "playlist/".Length, 22);
                await TryQueueSpotifyPlaylistAsync(userId, playlistId).ConfigureAwait(false);
            }
            else if (uriString.Contains("spotify:user:") && uriString.Contains("playlist:"))
            {
                var playlistIndex = uriString.IndexOf("playlist:", StringComparison.OrdinalIgnoreCase);

                var startIndex = "spotify:user:".Length;
                var userId = uriString.Substring(startIndex, playlistIndex - startIndex - 1);
                var playlistId = uriString.Substring(playlistIndex + "playlist:".Length, 22);
                await TryQueueSpotifyPlaylistAsync(userId, playlistId).ConfigureAwait(false);
            }

            // Handle Spotify Track
            else if (spotifyTrackIndex > 0)
            {
                var id = uriString.Substring(spotifyTrackIndex + "track/".Length, 22);
                await TryQueueSpotifyTrackAsync(id).ConfigureAwait(false);
            }
            else if (uriString.Contains("spotify:track:"))
            {
                var id = uriString.Substring("spotify:track:".Length, 22);
                await TryQueueSpotifyTrackAsync(id).ConfigureAwait(false);
            }

            // Handle Spotify Albums
            else if (spotifyAlbumIndex > 0)
            {
                var id = uriString.Substring(spotifyAlbumIndex + "album/".Length, 22);
                await TryQueueSpotifyAlbumAsync(id);
            }
            else if (uriString.Contains("spotify:album:"))
            {
                var id = uriString.Substring("spotify:album:".Length, 22);
                await TryQueueSpotifyAlbumAsync(id);
            }
        }

        [Command("play"), Priority(0)]
        public async Task FindAndDisplayYoutubeOptionsAsync([Remainder]string query)
        {
            if (!(Context.User is IGuildUser user) || user.VoiceChannel == null) return;

            var request = _youTubeService.Search.List("id,snippet");
            request.Q = query;
            var response = await request.ExecuteAsync().ConfigureAwait(false);

            var options = new List<Uri>();
            var optionsMessage = new StringBuilder();

            foreach (var searchResult in response.Items)
            {
                if (searchResult.Id.Kind == "youtube#video")
                {
                    optionsMessage.Append(
                        $"{response.Items.IndexOf(searchResult) + 1} - `{searchResult.Snippet.Title}` (Video)" +
                        Environment.NewLine);
                    options.Add(new Uri("https://www.youtube.com/watch?v=" + searchResult.Id.VideoId));
                }
                else if (searchResult.Id.Kind == "youtube#playlist")
                {
                    optionsMessage.Append(
                        $"{response.Items.IndexOf(searchResult) + 1} - `{searchResult.Snippet.Title}` (Playlist)" +
                        Environment.NewLine);
                    options.Add(new Uri("https://www.youtube.com/watch?list=" + searchResult.Id.PlaylistId));
                }
            }

            _musicSearches.AddSearch(user, options.ToArray());
            await ReplyAsync(optionsMessage.ToString()).ConfigureAwait(false);
        }

        [Command("queue")]
        public async Task DisplayQueueAsync()
        {
            var fileNames = _audioService.GetQueuedFiles(Context.Guild)?
                .Select(Path.GetFileNameWithoutExtension)
                .ToArray();

            if (fileNames == null)
            {
                await ReplyAsync("The queue is empty").ConfigureAwait(false);
                return;
            }

            var titles = (await TryGetYoutubeTitlesAsync(fileNames).ConfigureAwait(false)).ToArray();

            var message = new StringBuilder();

            for (var i = 0; i < fileNames.Length; i++)
            {
                var addition = $"{i + 1} - *{titles[i] ?? fileNames[i]}*" + Environment.NewLine;
                if ((message + addition).Length > 1997)
                {
                    message.Append("...");
                    break;
                }
                message.Append(addition);
            }

            if (string.IsNullOrWhiteSpace(message.ToString())) await ReplyAsync("The queue is empty").ConfigureAwait(false);
            else await ReplyAsync($"{message}").ConfigureAwait(false);
        }

        [Command("Dequeue")]
        public Task DequeueAsync(int position)
        {
            _audioService.Dequeue(Context.Guild, position - 1);
            return Task.CompletedTask;
        }

        [Command("Skip")]
        public Task SkipAsync()
        {
            _audioService.Skip(Context.Guild);
            return Task.CompletedTask;
        }

        [Command("KillQueue")]
        public Task StopGuildAudioAsync()
        {
            return _audioService.LeaveAudioAsync(Context.Guild);
        }

        private async Task<IEnumerable<string>> TryGetYoutubeTitlesAsync(IEnumerable<string> names)
        {
            var namesArray = names.ToArray();
            var request = _youTubeService.Videos.List("id,snippet");
            request.Id = string.Join(',', namesArray);
            var response = await request.ExecuteAsync().ConfigureAwait(false);

            var videoTitles = new string[namesArray.Length];
            for (var i = 0; i < namesArray.Length; i++)
            {
                var video = response.Items.FirstOrDefault(item => item.Id == namesArray[i]);
                videoTitles[i] = video?.Snippet.Title;
            }

            return videoTitles;
        }

        private async Task TryQueueSpotifyTrackAsync(string id)
        {
            var spotifyClient = await GetSpotifyClientAsync().ConfigureAwait(false);

            if (spotifyClient == null)
            {
                await ReplyAsync(LinkSpotify).ConfigureAwait(false);
                return;
            }

            var track = await spotifyClient.GetTrackAsync(id).ConfigureAwait(false);
            var youtubeId = await FindYoutubeIdAsync(track.Album.Name + " " + track.Name).ConfigureAwait(false);
            if (youtubeId == null) return;
            await VerifyAndQueueAsync(youtubeId).ConfigureAwait(false);
        }

        private async Task TryQueueSpotifyPlaylistAsync(string userId, string playlistId)
        {
            var spotifyClient = await GetSpotifyClientAsync().ConfigureAwait(false);

            if (spotifyClient == null)
            {
                await ReplyAsync(LinkSpotify).ConfigureAwait(false);
                return;
            }

            var playlist = await spotifyClient.GetPlaylistAsync(userId, playlistId)
                .ConfigureAwait(false);

            foreach (var playlistTrack in playlist.Tracks.Items)
            {
                var youtubeId = await FindYoutubeIdAsync(playlistTrack.Track.Name + " " + playlistTrack.Track.Artists.FirstOrDefault()?.Name).ConfigureAwait(false);
                if (youtubeId == null) continue;
                await VerifyAndQueueAsync(youtubeId).ConfigureAwait(false);
            }
        }

        private async Task TryQueueSpotifyAlbumAsync(string albumId)
        {
            var spotifyClient = await GetSpotifyClientAsync().ConfigureAwait(false);

            if (spotifyClient == null)
            {
                await ReplyAsync(LinkSpotify).ConfigureAwait(false);
                return;
            }

            var album = await spotifyClient.GetAlbumAsync(albumId);

            foreach (var track in album.Tracks.Items)
            {
                var youtubeId = await FindYoutubeIdAsync(track.Name + " " + track.Artists.FirstOrDefault()?.Name).ConfigureAwait(false);
                await VerifyAndQueueAsync(youtubeId).ConfigureAwait(false);
            }
        }

        private async Task<string> FindYoutubeIdAsync(string searchQ)
        {
            var request = _youTubeService.Search.List("id,snippet");
            request.Q = searchQ;
            var response = await request.ExecuteAsync().ConfigureAwait(false);
            return response.Items.First().Id.VideoId;
        }

        private async Task<IEnumerable<string>> GetVideoIdsAsync(string playlistId)
        {
            var listRequest = _youTubeService.PlaylistItems.List("id,contentDetails");
            listRequest.PlaylistId = playlistId;
            listRequest.MaxResults = 50;
            var playlistItems = (await listRequest.ExecuteAsync().ConfigureAwait(false)).Items;
            return playlistItems.Select(pli => pli.ContentDetails?.VideoId).Where(id => id != null);
        }

        private async Task<double?> VideoLengthAsync(string id)
        {
            var listRequest = _youTubeService.Videos.List("id,contentDetails");
            listRequest.Id = id;
            var response = await listRequest.ExecuteAsync().ConfigureAwait(false);

            if (response.Items.Count != 1) return null;

            var video = response.Items.Single();
            return XmlConvert.ToTimeSpan(video.ContentDetails.Duration).TotalMinutes;
        }

        private async Task VerifyAndQueueAsync(string id)
        {
            var length = (await VideoLengthAsync(id).ConfigureAwait(false));

            if (!length.HasValue)
            {
                await ReplyAsync("The requested video could not be found!").ConfigureAwait(false);
                return;
            }

            if (length > 10)
            {
                await ReplyAsync("The maximum length is 10 minutes!").ConfigureAwait(false);
            }

            else await PlayYoutubeSongByIdAsync(id).ConfigureAwait(false);
        }

        private async Task VerifyAndQueueAsync(IEnumerable<string> ids)
        {
            foreach (var id in ids)
            {
                var length = await VideoLengthAsync(id).ConfigureAwait(false);
                if (!length.HasValue || length > 10) continue;

                await PlayYoutubeSongByIdAsync(id).ConfigureAwait(false);
            }
        }

        private async Task PlayYoutubeSongByIdAsync(string id)
        {
            var output = $"{Config.DataDirectory}/youtubeDownloads/{id}.mp4";

            if (!File.Exists(output))
            {
                _youtubeDl.Options.FilesystemOptions.Output = output;
                _youtubeDl.Options.PostProcessingOptions.ExtractAudio = true;

                var process = _youtubeDl.Download($"https://www.youtube.com/watch?v={id}");
                while (true)
                {
                    await Task.Delay(100).ConfigureAwait(false);
                    if (process.HasExited) break;
                }
            }

            if (Context.User is IVoiceState state && state.VoiceChannel != null)
            {
                await _audioService.QueueFileAsync(
                    Context.Channel as ITextChannel,
                    state.VoiceChannel,
                    output).ConfigureAwait(false);
            }
        }
    }
}
