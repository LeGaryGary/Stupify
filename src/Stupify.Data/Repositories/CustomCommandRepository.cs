using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Stupify.Data.CustomCommandBuiltIns;
using Stupify.Data.SQL;
using Stupify.Data.SQL.Models;

namespace Stupify.Data.Repositories
{
    internal class CustomCommandRepository : ICustomCommandRepository
    {
        private readonly BotContext _botContext;
        private static IEnumerable<BuiltInCommand> _builtInCommands = null;

        private IServiceProvider _serviceProvider;
        public IServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider != null) return _serviceProvider;

                _serviceProvider = new ServiceCollection()
                    .AddSingleton<Random>()
                    .BuildServiceProvider();

                return _serviceProvider;
            }
        }

        public CustomCommandRepository(BotContext botContext)
        {
            _botContext = botContext;
        }

        public async Task<string> GetCommandAsync(IGuildUser user, string commandTag)
        {
            var guildId = (long) user.GuildId;
            var command = await _botContext.CustomCommands
                .Where(c => c.Server.DiscordGuildId == guildId)
                .FirstOrDefaultAsync(c => c.CommandTag == commandTag).ConfigureAwait(false);

            return command?.Command;
        }

        public async Task AddCommandAsync(IGuildUser user, string commandTag, string command)
        {
            var guildId = (long) user.GuildId;
            var dbServer = await _botContext.Servers.FirstOrDefaultAsync(s => s.DiscordGuildId == guildId).ConfigureAwait(false);
            _botContext.CustomCommands.Add(new CustomCommand
            {
                CommandTag = commandTag,
                Command = command,
                Server = dbServer
            });
            await _botContext.SaveChangesAsync().ConfigureAwait(false);
        }

        public async Task<string[]> ListServerCommandsAsync(IGuildUser user)
        {
            var guildId = (long) user.GuildId;
            var commands = _botContext.CustomCommands.Where(command => command.Server.DiscordGuildId == guildId);
            return (await commands.ToListAsync().ConfigureAwait(false)).Select(c => c.CommandTag).ToArray();
        }

        public async Task ExecuteAsync(ICommandContext context, int argPos)
        {
            try
            {
                var commandRequest = context.Message.Content.Substring(argPos).Split(' ');

                if (commandRequest.Length == 0) return;

                var commandTag = commandRequest[0];
                var args = commandRequest.Skip(1).ToArray();
                var command = await GetCommandAsync(context.User as IGuildUser, commandTag).ConfigureAwait(false);

                command = ReplaceArgs(command, args);
                command = Evaluate(command);

                await context.Channel.SendMessageAsync(command).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                await context.Channel.SendMessageAsync(e.Message).ConfigureAwait(false);
            }
        }

        private string Evaluate(string command)
        {
            var modifiedCommand = command;

            foreach (var builtInCommand in GetBuiltInCommands())
            {
                var i = modifiedCommand.IndexOf(builtInCommand.Tag, StringComparison.Ordinal);
                if (i < 0) continue;

                var commandAfterBuiltIn = modifiedCommand.Substring(i);
                GetArgsBoundaries(commandAfterBuiltIn, '\\', out var argStart, out var argEnd);

                var commandArgsString = commandAfterBuiltIn.Substring(argStart + 1, argEnd - argStart - 1);
                var args = SplitArgs(commandArgsString, ',', '\\');
                args = args.Select(Evaluate).ToArray();

                var result = builtInCommand.Execute(args);
                modifiedCommand = modifiedCommand.Replace(modifiedCommand.Substring(i, argEnd + 1), result);
            }

            return modifiedCommand;
        }

        private string[] SplitArgs(string input, char splitter, char escape)
        {
            var inputChars = input.ToCharArray();
            var lastSplit = 0;
            var depth = 0;
            var args = new List<string>();

            for (var i = 0; i < inputChars.Length; i++)
            {
                if (inputChars[i] == splitter && (i == 0 || inputChars[i - 1] != escape) && depth == 0)
                {
                    args.Add(input.Substring(lastSplit, i - lastSplit));
                    lastSplit = i + 1;
                }
                else if (inputChars[i] == '(' && (i == 0 || inputChars[i - 1] != escape))
                {
                    depth++;
                }
                else if (inputChars[i] == ')' && (i == 0 || inputChars[i - 1] != escape))
                {
                    depth--;
                }
            }

            args.Add(input.Substring(lastSplit));

            return args.ToArray();
        }
        
        private void GetArgsBoundaries(string input, char escape, out int start, out int end)
        {
            start = 0;
            var depth = 0;
            var inputChars = input.ToCharArray();
            for (var i = 0; i < inputChars.Length; i++)
            {
                if (inputChars[i] == '(' && (i == 0 || inputChars[i - 1] != escape))
                {
                    if (start == 0) start = i;
                    else depth++;
                }
                else if (inputChars[i] == ')')
                {
                    if (depth == 0)
                    {
                        end = i;
                        return;
                    }
                    depth--;
                }
            }

            throw new InvalidOperationException();
        }

        private IEnumerable<BuiltInCommand> GetBuiltInCommands()
        {
            if (_builtInCommands != null) return _builtInCommands;

            var commandTypes = Assembly.GetAssembly(typeof(BuiltInCommand)).GetTypes()
                .Where(type => typeof(BuiltInCommand).IsAssignableFrom(type) && type != typeof(BuiltInCommand));

            var commands = commandTypes.Select(commandType => (BuiltInCommand) commandType.GetConstructors().First().Invoke(new object[] {ServiceProvider}));
            _builtInCommands = commands;

            return _builtInCommands;
        }

        private static string ReplaceArgs(string command, string[] args)
        {
            int i;
            var replacedCommand = command;
            while ((i = replacedCommand.IndexOf("arg", StringComparison.Ordinal)) > 0)
            {
                var argNumber = int.Parse(replacedCommand.Substring(i + "arg".Length, 1));
                var arg = args[argNumber];
                replacedCommand = replacedCommand.Replace($"arg{argNumber}", arg);
            }

            return replacedCommand;
        }
    }
}
