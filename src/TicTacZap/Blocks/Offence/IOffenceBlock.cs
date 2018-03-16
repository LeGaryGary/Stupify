using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacZap.Blocks.Offence
{
    public interface IOffenceBlock : IBlock
    {
        void AttackSegment(Direction enemyDirection, Segment enemySegment);
    }
}
