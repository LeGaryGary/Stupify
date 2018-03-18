namespace TicTacZap
{
    public interface IHealth
    {
        int MaxHealth { get; }
        int Health { get; set; }

        string UnicodeHealth();
    }
}
