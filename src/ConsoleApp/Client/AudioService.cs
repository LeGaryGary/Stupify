using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Discord;
using Discord.Audio;

public class AudioService
{
    private readonly ConcurrentDictionary<ulong, (IAudioClient client, ConcurrentQueue<string> queue)> ConnectedChannels = new ConcurrentDictionary<ulong, (IAudioClient, ConcurrentQueue<string>)>();
    
    public async Task QueueFile(IGuild guild, IVoiceChannel target, string path)
    {
        if (ConnectedChannels.TryGetValue(guild.Id, out var audio))
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

        if (ConnectedChannels.TryAdd(guild.Id, audio))
        {
            Task.Run(() => SendAudioAsync(guild));
        }
    }

    public async Task LeaveAudio(IGuild guild)
    {
        if (ConnectedChannels.TryRemove(guild.Id, out var audio))
        {
            await audio.client.StopAsync();
        }
    }
    
    public async Task SendAudioAsync(IGuild guild)
    {
        while (ConnectedChannels.TryGetValue(guild.Id, out var audio) && audio.queue.TryDequeue(out var path))
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
            catch{}
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