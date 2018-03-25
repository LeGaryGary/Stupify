using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Discord;
using Discord.Commands;
using Google.Apis.Services;
using NYoutubeDL;
using Google.Apis.YouTube.v3;

namespace StupifyConsoleApp.Commands.Modules
{
    public class MusicAndSound : ModuleBase<CommandContext>
    {
        private readonly AudioService _audioService;
        private readonly YoutubeDL _youtubeDl;
        private readonly YouTubeService _youTubeService;

        public MusicAndSound(AudioService audioService, YoutubeDL youtubeDl, YouTubeService youTubeService)
        {
            _audioService = audioService;
            _youtubeDl = youtubeDl;
            _youTubeService = youTubeService;
        }

        [Command("AirHorn", RunMode = RunMode.Async)]
        public async Task PlayAirHorn()
        {
            if (Context.User is IVoiceState state && state.VoiceChannel != null)
                await _audioService.QueueFile(Context.Guild, state.VoiceChannel, Directory.GetCurrentDirectory() + "\\AirHorn.mp3");
        }

        [Command("play", RunMode = RunMode.Async)]
        public async Task DownloadAndPlayFromYoutube(string youtubeUrl)
        {
            string id = null;
            try
            {
                var uri = new Uri(youtubeUrl);
                var query = HttpUtility.ParseQueryString(uri.Query);
                id = query["v"];
            }
            catch{}

            if (id == null)
            {
                await ReplyAsync("Unable to extract youtube video id");
                return;
            }

            if (await VideoLength(id) > 10)
            {
                await ReplyAsync("The maximum length is 10 minutes!");
                return;
            }

            var output = $"{Config.DataDirectory}/youtubeDownloads/{id}.mp4";

            if (!File.Exists(output))
            {
                _youtubeDl.Options.FilesystemOptions.Output = output;
                _youtubeDl.Options.PostProcessingOptions.ExtractAudio = true;

                var process = _youtubeDl.Download($"https://www.youtube.com/watch?v={id}");
                while (true)
                {
                    await Task.Delay(100);
                    if (process.HasExited) break;
                }
            }

            if (Context.User is IVoiceState state && state.VoiceChannel != null)
                await _audioService.QueueFile(Context.Guild, state.VoiceChannel, output);
        }

        private async Task<double> VideoLength(string id)
        {
            var listRequest = _youTubeService.Videos.List("id,contentDetails");
            listRequest.Id = id;
            var video = (await listRequest.ExecuteAsync()).Items.Single();
            return XmlConvert.ToTimeSpan(video.ContentDetails.Duration).TotalMinutes;
        }
    }
}
