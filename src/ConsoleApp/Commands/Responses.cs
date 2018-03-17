using TicTacZap;

namespace StupifyConsoleApp.Commands
{
    public static class Responses
    {
        public const string SegmentOwnershipProblem = "You don't own a segment with this Id!";

        public const string TemplateOwnershipProblem = "You don't own a template with this Id!";

        public static readonly string SelectSegmentMessage =
            $"Please select a segment with {Config.CommandPrefix} segment [segmentId]";

        public const string NoSuchBlock = "No such block exists";

        public static readonly string SelectTemplateMessage = 
            $"Please select a template with {Config.CommandPrefix} Template [templateId]";

        public static readonly string ShopAdvisoryMessage =
            $"Please buy the item you are trying to use! `{Config.CommandPrefix} shop` and `{Config.CommandPrefix} buy [type] [quantity]`";

        public static string NotEnoughUnits(decimal price)
        {
            return $"Come back when you have more money (you need {price} {Resource.Unit} to buy this)";
        }
    }
}