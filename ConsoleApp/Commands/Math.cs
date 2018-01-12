using System.Threading.Tasks;
using Discord.Commands;

namespace StupifyConsoleApp.Commands
{
    public class Math : ModuleBase<SocketCommandContext>
    {
        [Command("add")]
        [Summary("Adds up a list of comma seperated numbers")]
        public async Task AddAsync()
        {
            await ReplyAsync("This does nothing...");
        }

        [Command("echo")]
        [Summary("Echos a message.")]
        public async Task SayAsync([Remainder] [Summary("The text to echo")] string echo)
        {
            await ReplyAsync(echo);
        }
    }
}
