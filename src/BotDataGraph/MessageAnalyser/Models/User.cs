namespace BotDataGraph.MessageAnalyser.Models
{
    public class User
    {
        private string userId;

        public ulong UserId
        {
            get => ulong.Parse(userId);
            set => userId = value.ToString();
        }

        public object Parameters()
        {
            return new {userId};
        }
    }
}