using System.Threading.Tasks;
using Discord;

public interface ICustomTextRepository
{
    Task<string> GetBanTextAsync(IGuildUser userToBan, int? daysOfMessagesToDelete, string banReason);
    Task<string> GetBanTextAsync(ulong guildId);
    Task SetBanTextAsync(ulong guildId, string banText);

    Task<string> GetKickTextAsync(IGuildUser userToKick, string kickReason);
    Task<string> GetKickTextAsync(ulong guildId);
    Task SetKickTextAsync(ulong guildId, string kickText);

    Task<string> GetWelcomeTextAsync(IGuildUser userThatJoined);
    Task<string> GetWelcomeTextAsync(ulong guildId);
    Task SetWelcomeTextAsync(ulong guildId, string welcomeText);

    Task<string> GetLeaveTextAsync(IGuildUser userThatLeft);
    Task<string> GetLeaveTextAsync(ulong guildId);
    Task SetLeaveTextAsync(ulong guildId, string leaveText);

    Task<string> GetBlockedWordTextAsync(IGuildUser user);
    Task<string> GetBlockedWordTextAsync(ulong guildId);
    Task SetBlockedWordTextAsync(ulong guildId, string blockedWordText);
}