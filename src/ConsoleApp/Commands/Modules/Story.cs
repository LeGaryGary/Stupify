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
        public async Task StoryStartAsync([Remainder]string line)
        {
            if (Context.User is IGuildUser user &&
                await _storyRepository.StartNewStoryAsync(user, line).ConfigureAwait(false))
            {
                await ReplyAsync(
                    $"The story begins here, who knows where it will go! Use the command {Config.CommandPrefix} AndThen [line]" +
                    Environment.NewLine +
                    $"To end the story use {Config.CommandPrefix} theEnd, good luck!").ConfigureAwait(false);
            }
            else
            {
                // TODO: Make this get the current story and expand on why it is unable
                await ReplyAsync("Unable to start story").ConfigureAwait(false);
            }
        }

        [Command("AndThen")]
        public async Task AddStoryPartAsync([Remainder]string line)
        {
            if (Context.User is IGuildUser user &&
                await _storyRepository.AddToCurrentStoryAsync(user, line).ConfigureAwait(false))
            {
                await ReplyAsync("Your part has been added!").ConfigureAwait(false);
            }
            else
            {
                // TODO: Make this get the current story and expand on why it is unable
                await ReplyAsync("Unable to add your part").ConfigureAwait(false);
            }
        }

        [Command("TheEnd")]
        public async Task StoryEndAsync()
        {
            if (await _storyRepository.EndCurrentStoryAsync(Context.Guild).ConfigureAwait(false))
            {
                await ReplyAsync("The end!").ConfigureAwait(false);
            }
            else
            {
                var story = await _storyRepository.GetCurrentStoryAsync(Context.Guild).ConfigureAwait(false);
                if (story == null)
                {
                    await ReplyAsync($"There's no story in progress! try {Config.CommandPrefix} BeginStory")
                        .ConfigureAwait(false);
                    return;
                }

                if (!story.AtLeastMinLength())
                {
                    await ReplyAsync("This story is too short to end, please continue!").ConfigureAwait(false);
                    return;
                }

                throw new InvalidOperationException();
            }
        }

        [Command("TellMeAStory")]
        public async Task ReplayStoryAsync()
        {
            var story = await _storyRepository.RandomStoryAsync(Context.Guild).ConfigureAwait(false);
            if (story != null)
            {
                await ReplyAsync(story.Content).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync(
                        $"What... You want me to make one up?? (This server doesn't have any! (((Try {Config.CommandPrefix} beginstory))))")
                    .ConfigureAwait(false);
            }
        }
    }
}