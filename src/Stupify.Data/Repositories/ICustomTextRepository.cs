using System.Threading.Tasks;
using Discord;

public interface ICustomTextRepository
{
    Task<string> GetBanTextAsync(IGuildUser userToBan, int? daysOfMessagesToDelete, string banReason);
    Task SetBanTextAsync(ulong guildId, string banText);

    Task<string> GetKickTextAsync(IGuildUser userToKick, string kickReason);
    Task SetKickTextAsync(ulong guildId, string kickText);

    Task<string> GetWelcomeTextAsync(IGuildUser userThatJoined);
    Task SetWelcomeTextAsync(ulong guildId, string welcomeText);

    Task<string> GetLeaveTextAsync(IGuildUser userThatLeft);
    Task SetLeaveTextAsync(ulong guildId, string leaveText);
}