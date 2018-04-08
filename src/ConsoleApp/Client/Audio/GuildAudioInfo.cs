using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Discord;
using Discord.Audio;

namespace StupifyConsoleApp.Client.Audio
{
    internal class GuildAudioInfo
    {
        public GuildAudioInfo(ITextChannel infoChannel, IAudioClient client)
        {
            InfoChannel = infoChannel;
            Client = client;
            Ffmpeg = null;
            Queue = new ConcurrentQueue<string>();
        }

        public ITextChannel InfoChannel { get; private set; }
        public IAudioClient Client { get; private set; }
        public ConcurrentQueue<string> Queue { get; set; }
        public Process Ffmpeg { get; set; }
    }
}
