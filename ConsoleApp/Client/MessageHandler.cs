﻿using System.Diagnostics;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Client
{
    internal static class MessageHandler
    {

        internal static async Task Handle(SocketMessage messageParam)
        {
            if (!(messageParam is SocketUserMessage message) || messageParam.Author.IsBot) return;
            
            var argPos = 0;
            var context = new SocketCommandContext(ClientManager.Client, message);

            using (var db = new BotContext())
            {
                var serverUser = await db.GetServerUserAsync(context.User.Id, context.Guild.Id, true);
                if (serverUser.Muted)
                {
                    var deleteTask = context.Message.DeleteAsync();
                    var dmChannel = await context.User.GetOrCreateDMChannelAsync();
                    await dmChannel.SendMessageAsync("You are muted, congrats muggle, you did it");
                    await deleteTask;
                    return;
                }
            }
            
            if (!(message.HasStringPrefix(Config.CommandPrefix, ref argPos) ||
                  message.HasMentionPrefix(ClientManager.Client.CurrentUser, ref argPos))) return;

            var sw = new Stopwatch();
            sw.Start();
            var result = await ClientManager.Commands.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
                if (result.Error == CommandError.UnknownCommand)
                    await context.Channel.SendMessageAsync("Command not found!");
                else
                    await ClientManager.LogAsync(result.ErrorReason);
            sw.Stop();
            await ClientManager.LogAsync("This command took "+sw.ElapsedMilliseconds+"ms", true);
        }
    }
}