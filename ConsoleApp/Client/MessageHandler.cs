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
            // Don't process the command if it was a System Message
            if (!(messageParam is SocketUserMessage message)) return;


            // Create a number to track where the prefix ends and the command begins
            var argPos = 0;

            // Create a Command Context
            var context = new SocketCommandContext(ClientManager.Client, message);

            AddStats(context);

            if (context.User.Id == ClientManager.Client.CurrentUser.Id) return;

            // Determine if the message is a command, based on if it starts with '!' or a mention prefix
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(ClientManager.Client.CurrentUser, ref argPos))) return;
            
            //Check is client is ready
            if (!ClientManager.IsReady)
            {
                await context.Channel.SendMessageAsync("Bot is starting up!");
                return;
            }

            // Execute the command. (result does not indicate a return value, 
            // rather an object stating if the command executed successfully)
            var result = await ClientManager.Commands.ExecuteAsync(context, argPos);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync("Internal error");
        }

        private static void AddStats(SocketCommandContext context)
        {
            using (var db = new BotContext())
            {
                db.GetServerUser((long) context.User.Id, (long) context.Guild.Id);
            }
        }
    }
}