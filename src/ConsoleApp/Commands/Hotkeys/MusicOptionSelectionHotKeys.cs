using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Discord;
using Discord.Commands;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.Commands.Hotkeys
{
    internal class MusicOptionSelectionHotKey1 : IHotkey
    {
        private readonly MusicSearches _musicSearches;
        private readonly CommandService _commandService;

        public MusicOptionSelectionHotKey1(MusicSearches musicSearches, CommandService commandService)
        {
            _musicSearches = musicSearches;
            _commandService = commandService;
        }

        public char Key => '1';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            if (!(context.User is IGuildUser user) || !_musicSearches[user]) return false;

            await _commandService.ExecuteAsync(context, $"play {_musicSearches[user, 0]}", Config.ServiceProvider);
            return true;
        }
    }

    internal class MusicOptionSelectionHotKey2 : IHotkey
    {
        private readonly MusicSearches _musicSearches;
        private readonly CommandService _commandService;

        public MusicOptionSelectionHotKey2(MusicSearches musicSearches, CommandService commandService)
        {
            _musicSearches = musicSearches;
            _commandService = commandService;
        }

        public char Key => '2';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            if (!(context.User is IGuildUser user) || !_musicSearches[user]) return false;

            await _commandService.ExecuteAsync(context, $"play {_musicSearches[user, 1]}", Config.ServiceProvider);
            return true;
        }
    }

    internal class MusicOptionSelectionHotKey3 : IHotkey
    {
        private readonly MusicSearches _musicSearches;
        private readonly CommandService _commandService;

        public MusicOptionSelectionHotKey3(MusicSearches musicSearches, CommandService commandService)
        {
            _musicSearches = musicSearches;
            _commandService = commandService;
        }

        public char Key => '3';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            if (!(context.User is IGuildUser user) || !_musicSearches[user]) return false;

            await _commandService.ExecuteAsync(context, $"play {_musicSearches[user, 2]}", Config.ServiceProvider);
            return true;
        }
    }

    internal class MusicOptionSelectionHotKey4 : IHotkey
    {
        private readonly MusicSearches _musicSearches;
        private readonly CommandService _commandService;

        public MusicOptionSelectionHotKey4(MusicSearches musicSearches, CommandService commandService)
        {
            _musicSearches = musicSearches;
            _commandService = commandService;
        }

        public char Key => '4';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            if (!(context.User is IGuildUser user) || !_musicSearches[user]) return false;

            await _commandService.ExecuteAsync(context, $"play {_musicSearches[user, 3]}", Config.ServiceProvider);
            return true;
        }
    }

    internal class MusicOptionSelectionHotKey5 : IHotkey
    {
        private readonly MusicSearches _musicSearches;
        private readonly CommandService _commandService;

        public MusicOptionSelectionHotKey5(MusicSearches musicSearches, CommandService commandService)
        {
            _musicSearches = musicSearches;
            _commandService = commandService;
        }

        public char Key => '5';

        public async Task<bool> ExecuteAsync(ICommandContext context)
        {
            if (!(context.User is IGuildUser user) || !_musicSearches[user]) return false;

            await _commandService.ExecuteAsync(context, $"play {_musicSearches[user, 4]}", Config.ServiceProvider);
            return true;
        }
    }
}
