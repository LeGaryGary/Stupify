using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Discord;
using Discord.Commands;
using NYoutubeDL;
using Google.Apis.YouTube.v3;
using StupifyConsoleApp.Client.Audio;

namespace StupifyConsoleApp.Commands.Modules
{
    public class MusicAndSound : ModuleBase<CommandContext>
    {
        private readonly AudioService _audioService;
        private readonly YoutubeDL _youtubeDl;
        private readonly YouTubeService _youTubeService;
        private readonly MusicSearches _musicSearches;

        public MusicAndSound(AudioService audioService, YoutubeDL youtubeDl, YouTubeService youTubeService, MusicSearches musicSearches)
        {
            _audioService = audioService;
            _youtubeDl = youtubeDl;
            _youTubeService = youTubeService;
            _musicSearches = musicSearches;
        }

        [Command("AirHorn", RunMode = RunMode.Async)]
        public async Task PlayAirHornAsync()
        {
            if (Context.User is IVoiceState state && state.VoiceChannel != null)
                await _audioService.QueueFile(Context.Channel as ITextChannel, state.VoiceChannel, Directory.GetCurrentDirectory() + "\\AirHorn.mp3").ConfigureAwait(false);
        }

        [Command("play", RunMode = RunMode.Async), Priority(1)]
        public async Task DownloadAndPlayFromYoutubeAsync(Uri youtubeUrl)
        {
            var query = HttpUtility.ParseQueryString(youtubeUrl.Query);

            var listValues = query.GetValues("list");
            var vValues = query.GetValues("v");

            if (listValues != null)
            {
                var playlistId = listValues.First();
                var ids = await GetVideoIdsAsync(playlistId).ConfigureAwait(false);
                foreach (var id in ids)
                {
                    var length = await VideoLengthAsync(id).ConfigureAwait(false);
                    if (!length.HasValue || length > 10) continue;

                    await PlayYoutubeSongByIdAsync(id).ConfigureAwait(false);
                }
            }
            else if (vValues != null)
            {
                var id = vValues.First();
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
        }

        [Command("play"), Priority(0)]
        public async Task FindAndDisplayYoutubeOptionsAsync([Remainder]string query)
        {
            if (!(Context.User is IGuildUser user)) return;

            var request = _youTubeService.Search.List("id,snippet");
            request.Q = query;
            var response = await request.ExecuteAsync().ConfigureAwait(false);

            var options = new List<Uri>();
            var optionsMessage = string.Empty;

            foreach (var searchResult in response.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        optionsMessage += $"{response.Items.IndexOf(searchResult) + 1} - `{searchResult.Snippet.Title}` (Video)" + Environment.NewLine;
                        options.Add(new Uri("https://www.youtube.com/watch?v=" + searchResult.Id.VideoId));
                        break;

                    case "youtube#playlist":
                        optionsMessage += $"{response.Items.IndexOf(searchResult) + 1} - `{searchResult.Snippet.Title}` (Playlist)" + Environment.NewLine;
                        options.Add(new Uri("https://www.youtube.com/watch?list=" + searchResult.Id.PlaylistId));
                        break;
                }
            }

            _musicSearches.AddSearch(user, options.ToArray());
            await ReplyAsync(optionsMessage).ConfigureAwait(false);
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

            var message = string.Empty;

            for (var i = 0; i < fileNames.Length; i++)
            {
                var addition = $"{i} - *{titles[i] ?? fileNames[i]}*" + Environment.NewLine;
                if ((message + addition).Length > 1997)
                {
                    message += "...";
                    break;
                }
                message += addition;
            }

            if (string.IsNullOrWhiteSpace(message)) await ReplyAsync("The queue is empty").ConfigureAwait(false);
            else await ReplyAsync($"{message}").ConfigureAwait(false);
        }

        [Command("Dequeue")]
        public void Dequeue(int position)
        {
            _audioService.Dequeue(Context.Guild, position);
        }

        [Command("Skip")]
        public void Skip()
        {
            _audioService.Skip(Context.Guild);
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
                await _audioService.QueueFile(
                    Context.Channel as ITextChannel,
                    state.VoiceChannel,
                    output).ConfigureAwait(false);
            }
        }
    }
}
