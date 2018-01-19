using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Microsoft.EntityFrameworkCore;
using StupifyConsoleApp.DataModels;

namespace StupifyConsoleApp.Commands
{
    public class StoryModule : ModuleBase<SocketCommandContext>
    {
        [Command("beginstory")]
        public async Task StoryStart()
        {
            using (var db = new BotContext())
            {
                var server = db.Servers.First(s => (ulong) s.DiscordGuildId == Context.Guild.Id);
                if (server.StoryInProgress)
                {
                    await ReplyAsync("Theres already a story in progress!!");
                    return;
                }

                server.StoryInProgress = true;
                var serverStory = new ServerStory
                {
                    Server = server,
                    StartTime = DateTime.Now,
                    StoryInitiatedBy = await db.GetServerUserAsync(Context.User.Id, Context.Guild.Id),
                };
                db.ServerStories.Add(serverStory);
                db.ServerStoryParts.Add(new ServerStoryPart
                {
                    ServerStory = serverStory,
                    PartNumber = 0,
                    TimeOfAddition = DateTime.Now,
                    PartAuthor = await db.GetServerUserAsync(Context.User.Id, Context.Guild.Id),
                    Part = ""
                });
                await db.SaveChangesAsync();
                await ReplyAsync(
                    $"The story begins here, who knows where it will go! Use the command {Config.CommandPrefix} andthen {{Your part of the story!}}" + Environment.NewLine +
                    $"To end the story use {Config.CommandPrefix} theend, goodluck!");

            }
        }

        [Command("andthen")]
        public async Task AddStoryPart([Remainder] string line)
        {
            using (var db = new BotContext())
            {
                var server = await db.Servers.FirstAsync(s => (ulong) s.DiscordGuildId == Context.Guild.Id);
                
                if (!server.StoryInProgress)
                {
                    await ReplyAsync($"Theres no story in progress! try {Config.CommandPrefix} beginstory");
                    return;
                }

                var serverUser = await db.GetServerUserAsync(Context.User.Id, Context.Guild.Id);
                var story = await db.GetLatestServerStoryAsync((long)Context.Guild.Id);
                var lastPart = await db.GetLastestServerStoryPartAsync(story);

                var timeSpan = DateTime.Now - lastPart.TimeOfAddition;
                if (lastPart.PartAuthor.ServerUserId == serverUser.ServerUserId &&
                    timeSpan < TimeSpan.FromMinutes(1))
                {
                    await ReplyAsync(
                        $"Please give other people a chance! Or at the very least, wait another {60-timeSpan.Seconds} Seconds!");
                    return;
                }

                db.ServerStoryParts.Add(new ServerStoryPart
                {
                    ServerStory = story,
                    Part = line,
                    PartAuthor = serverUser,
                    PartNumber = lastPart.PartNumber + 1,
                    TimeOfAddition = DateTime.Now
                });
                await db.SaveChangesAsync();
                await ReplyAsync("ADDED");
            }
        }

        [Command("theend")]
        public async Task StoryEnd()
        {
            using (var db = new BotContext())
            {
                var server = await db.Servers.FirstAsync(s => (ulong) s.DiscordGuildId == Context.Guild.Id);

                if (!server.StoryInProgress)
                {
                    await ReplyAsync($"Theres no story in progress! try {Config.CommandPrefix} beginstory");
                    return;
                }

                var story = await db.GetLatestServerStoryAsync((long)Context.Guild.Id);
                var partsCount = await db.ServerStoryParts
                    .CountAsync(ssp => ssp.ServerStory.ServerStoryId == story.ServerStoryId);
                if (partsCount < 10)
                {
                    await ReplyAsync("This story is too short to end, please continue!");
                    return;
                }

                server.StoryInProgress = false;
                await db.SaveChangesAsync();
                await ReplyAsync("The end!");
            }
        }
    }
}
