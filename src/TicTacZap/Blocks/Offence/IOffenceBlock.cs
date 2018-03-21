namespace TicTacZap.Blocks.Offence
{
    public interface IOffenceBlock : IBlock
    {
        void AttackSegment(Direction enemyDirection, Segment enemySegment);
    }
}
