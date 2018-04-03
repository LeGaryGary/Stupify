using System;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Stupify.Data.Repositories;

namespace StupifyConsoleApp.Commands.Modules
{
    public class Story : ModuleBase<CommandContext>
    {
        private readonly IStoryRepository _storyRepository;

        public Story(IStoryRepository storyRepository)
        {
            _storyRepository = storyRepository;
        }

        [Command("BeginStory")]
        public async Task StoryStart(string line)
        {
            if (Context.User is IGuildUser user && await _storyRepository.StartNewStoryAsync(user, line))
            {
                await ReplyAsync(
                    $"The story begins here, who knows where it will go! Use the command {Config.CommandPrefix} AndThen [line]" +
                    Environment.NewLine +
                    $"To end the story use {Config.CommandPrefix} theend, good luck!");
            }
            else
            {
                // TODO: Make this get the current story and expand on why it is unable
                await ReplyAsync("Unable to start story");
            }
        }
        
        [Command("AndThen")]
        public async Task AddStoryPart(string line)
        {

            if (Context.User is IGuildUser user && await _storyRepository.AddToCurrentStoryAsync(user, line))
            {
                await ReplyAsync("Your part has been added!");
            }
            else
            {
                // TODO: Make this get the current story and expand on why it is unable
                await ReplyAsync("Unable to add your part");
            }
        }

        [Command("TheEnd")]
        public async Task StoryEnd()
        {
            if (await _storyRepository.EndCurrentStoryAsync(Context.Guild))
            {
                await ReplyAsync("The end!");
            }
            else
            {
                var story = await _storyRepository.GetCurrentStoryAsync(Context.Guild);
                if (story == null)
                {
                    await ReplyAsync($"There's no story in progress! try {Config.CommandPrefix} BeginStory");
                    return;
                }
                if (!story.AtLeastMinLength())
                {
                    await ReplyAsync("This story is too short to end, please continue!");
                    return;
                }
                throw new InvalidOperationException();
            }
        }

        [Command("TellMeAStory")]
        public async Task ReplayStory()
        {
            var story = await _storyRepository.RandomStoryAsync(Context.Guild);
            if (story != null)
            {
                await ReplyAsync(story.Content);
            }
            else
            {
                await ReplyAsync(
                    $"What... You want me to make one up?? (This server doesn't have any! (((Try {Config.CommandPrefix} beginstory))))");
            }
        }
    }
}