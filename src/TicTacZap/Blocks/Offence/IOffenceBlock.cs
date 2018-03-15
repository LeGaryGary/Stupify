using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacZap.Blocks.Offence
{
    interface IOffenceBlock : IBlock
    {
        void AttackSegment(Direction enemyDirection, Segment enemySegment);
    }
}
