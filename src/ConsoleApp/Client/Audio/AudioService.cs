using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Logging;

namespace StupifyConsoleApp.Client.Audio
{
    public class AudioService
    {
        private readonly ILogger<AudioService> _logger;
        private readonly YouTubeService _youTubeService;
        private readonly ConcurrentDictionary<ulong, GuildAudioInfo> _connectedChannels = new ConcurrentDictionary<ulong, GuildAudioInfo>();

        public AudioService(ILogger<AudioService> logger, YouTubeService youTubeService)
        {
            _logger = logger;
            _youTubeService = youTubeService;
        }

        public async Task QueueFile(ITextChannel guildChannel, IVoiceChannel target, string path)
        {
            if(guildChannel == null) return;
            if (_connectedChannels.TryGetValue(guildChannel.Guild.Id, out var audio))
            {
                audio.Queue.Enqueue(path);
                return;
            }

            if (target.Guild.Id != guildChannel.Guild.Id) return;

            var audioClient = await target.ConnectAsync();

            audio = new GuildAudioInfo(guildChannel, audioClient);
            audio.Queue.Enqueue(path);

            if (_connectedChannels.TryAdd(guildChannel.Guild.Id, audio))
            {
                #pragma warning disable CS4014
                Task.Run(() => SendAudioAsync(guildChannel.Guild));
                #pragma warning restore CS4014 
            }
        }

        private async Task SendAudioAsync(IGuild guild)
        {
            while (_connectedChannels.TryGetValue(guild.Id, out var audio) && audio.Queue.TryDequeue(out var path))
            {
                try
                {
                    var videoId = Path.GetFileNameWithoutExtension(path);
                    audio.InfoChannel?.SendMessageAsync(
                        string.Empty,
                        embed: await GetEmbedForYoutubeVideo(videoId));
                    using (var ffmpeg = CreateStream(path))
                    using (var stream = audio.Client.CreatePCMStream(AudioApplication.Music))
                    {
                        audio.Ffmpeg = ffmpeg;
                        try
                        {
                            await ffmpeg.StandardOutput.BaseStream.CopyToAsync(stream);
                        }
                        finally
                        {
                            await stream.FlushAsync();
                        }
                    }
                }
                catch(Exception e){ _logger.LogWarning(e, "Exception whilst attempting to stream the file {FilePath}", path);}
            }

            await LeaveAudioAsync(guild);
        }

        private async Task<Embed> GetEmbedForYoutubeVideo(string id)
        {
            var request = _youTubeService.Videos.List("id,snippet");
            request.Id = id;
            var response = await request.ExecuteAsync();
            if (response.Items.Count != 1) return null;
            var video = response.Items.Single();

            var embed = new EmbedBuilder
            {
                Title = "Now playing...",
                ImageUrl = video.Snippet.Thumbnails.High.Url,
                Url = $"https://www.youtube.com/watch?v{id}"
            };

            return embed.Build();
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                WorkingDirectory = Directory.GetCurrentDirectory(),
                FileName = "ffmpeg.exe",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }

        public async Task LeaveAudioAsync(IGuild guild)
        {
            if (_connectedChannels.TryRemove(guild.Id, out var audio))
            {
                await audio.Client.StopAsync().ConfigureAwait(false);
                audio.Ffmpeg.Kill();
            }
        }

        public IEnumerable<string> GetQueuedFiles(IGuild guild)
        {
            return _connectedChannels.TryGetValue(guild.Id, out var audio) ? audio.Queue : null;
        }

        public void Dequeue(IGuild guild, int position)
        {
            if (!_connectedChannels.TryGetValue(guild.Id, out var audio)) return;

            var list = audio.Queue.ToList();
            list.RemoveAt(position);
            var queue = new ConcurrentQueue<string>(list);
            audio.Queue = queue;
        }

        public void Skip(IGuild guild)
        {
            if (!_connectedChannels.TryGetValue(guild.Id, out var audio)) return;
            audio.Ffmpeg?.Kill();
        }
    }
}