

namespace StupifyConsoleApp.Commands
{
    public static class Responses
    {
        public static readonly string SegmentOwnershipProblem = "You don't own a segment with this Id!";
        public static readonly string SelectSegmentMessage = $"Please select a segment with {Config.CommandPrefix}segment [segmentId]";

        public static readonly string ShopAdvisoryMessage =
            $"Please buy the item you are trying to use! `{Config.CommandPrefix}shop` and `{Config.CommandPrefix}buy [type] [quantity]`";


    }
}
