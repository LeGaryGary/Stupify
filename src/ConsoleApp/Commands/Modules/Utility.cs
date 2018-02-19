using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using StupifyConsoleApp.Client;

namespace StupifyConsoleApp.Commands.Modules
{
    public class Utility : ModuleBase<SocketCommandContext>
    {
        private readonly List<string> _seenModules = new List<string>();

        [Command("HelpAll")]
        public async Task HelpAll()
        {
            var modules = ClientManager.Commands.Modules;
            var message = string.Empty;
            foreach (var module in modules)
            {
                if (_seenModules.Contains(module.Name)) continue;
                _seenModules.Add(module.Name);
                message += RenderModuleHelp(module, string.Empty, 0);
            }

            await ReplyAsync(message);
        }

        [Command("Help")]
        public async Task Help()
        {
            var modules = ClientManager.Commands.Modules;
            var message = string.Empty;
            foreach (var module in modules)
            {
                message += module.Name + Environment.NewLine;
            }
            await ReplyAsync(message);
        }

        [Command("Help")]
        public async Task Help(string moduleName)
        {
            var module = ClientManager.Commands.Modules.FirstOrDefault(m => string.Equals(m.Name, moduleName, StringComparison.OrdinalIgnoreCase));
            if (module == null)
            {
                await ReplyAsync($"This module doesn't exist, please use {Config.CommandPrefix}help to see a list of modules.");
                return;
            }
            await ReplyAsync(RenderModuleHelp(module, string.Empty, 0));
        }

        private string RenderModuleHelp(ModuleInfo module, string prefix, int tabNumber)
        {
            var str = GetTab(tabNumber) + module.Name + ":" + Environment.NewLine;
            foreach (var command in module.Commands)
            {
                if (prefix.Contains(command.Name))
                {
                    str += GetTab(tabNumber + 1) + $"-{command.Name}: `{Config.CommandPrefix}{prefix} {CommandParameterHelp(command)}`" + Environment.NewLine;
                    continue;
                }
                str += GetTab(tabNumber + 1) + $"-{command.Name}: `{Config.CommandPrefix}{prefix} {command.Name} {CommandParameterHelp(command)}`" + Environment.NewLine;
            }

            foreach (var subModule in module.Submodules)
            {
                if (_seenModules.Contains(subModule.Name)) continue;
                _seenModules.Add(subModule.Name);
                str += RenderModuleHelp(subModule, $"{prefix} {subModule.Name}", tabNumber + 1);
            }

            return str;
        }

        private string CommandParameterHelp(CommandInfo command)
        {
            return string.Join(" ", command.Parameters.Select(p => $"[{p.Name}]"));
        }

        private string GetTab(int tabNumber)
        {
            var str = string.Empty;
            for (var i = 0; i < tabNumber; i++)
            {
                str += "\t";
            }

            return str;
        }
    }
}
