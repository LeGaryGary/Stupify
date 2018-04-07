using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;
using Microsoft.Extensions.Logging;

namespace StupifyConsoleApp.Client
{
    public class AudioService
    {
        private readonly ILogger<AudioService> _logger;
        private readonly ConcurrentDictionary<ulong, (IAudioClient client, ConcurrentQueue<string> queue)> _connectedChannels = new ConcurrentDictionary<ulong, (IAudioClient, ConcurrentQueue<string>)>();

        public AudioService(ILogger<AudioService> logger)
        {
            _logger = logger;
        }

        public async Task QueueFile(IGuild guild, IVoiceChannel target, string path)
        {
            if (_connectedChannels.TryGetValue(guild.Id, out var audio))
            {
                audio.queue.Enqueue(path);
                return;
            }
            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            audio = (audioClient, new ConcurrentQueue<string>());
            audio.queue.Enqueue(path);

            if (_connectedChannels.TryAdd(guild.Id, audio))
            {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                Task.Run(() => SendAudioAsync(guild));
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            if (_connectedChannels.TryRemove(guild.Id, out var audio))
            {
                await audio.client.StopAsync();
            }
        }
    
        public async Task SendAudioAsync(IGuild guild)
        {
            while (_connectedChannels.TryGetValue(guild.Id, out var audio) && audio.queue.TryDequeue(out var path))
            {
                try
                {
                    using (var ffmpeg = CreateStream(path))
                    using (var stream = audio.client.CreatePCMStream(AudioApplication.Music))
                    {
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

            await LeaveAudio(guild);
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
    }
}