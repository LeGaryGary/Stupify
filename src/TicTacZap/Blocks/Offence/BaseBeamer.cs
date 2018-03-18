using System;
using System.Data;

namespace TicTacZap.Blocks.Offence
{
    public abstract class BaseBeamer : BaseBlock, IOffenceBlock, IEnergyConsumer
    {
        private readonly int _x;
        private readonly int _y;
        private readonly Func<int,int,Direction,Segment,(int x, int y)?> _targetLocator;

        public int EnergyConsumption { get; protected set; }
        protected int BeamPower;


        protected BaseBeamer(int xInSegment, int yInSegment, Func<int,int,Direction,Segment,(int x, int y)?> targetLocator)
        {
            _x = xInSegment;
            _y = yInSegment;
            _targetLocator = targetLocator ?? DefaultTargeter;
        }

        public void AttackSegment(Direction enemyDirection, Segment enemySegment)
        {
            var target = _targetLocator(_x, _y, enemyDirection, enemySegment);

            if (!target.HasValue) return;
            var (x, y) = target.Value;
            
            if ((enemySegment.Blocks[x, y].Health -= BeamPower) > 0) return;

            enemySegment.Blocks[x, y].DestroyThis();
            enemySegment.Blocks[x, y] = null;
            enemySegment.UpdateSegmentOutput();
        }

        private static (int x, int y)? DefaultTargeter(int x, int y, Direction enemyDirection, Segment enemySegment)
        {
            int? distance = null;
            (int, int)? target = null;

            var blocks = enemySegment.Blocks;
            for (var iy = 0; iy < enemySegment.Blocks.GetLength(1); iy++)
            for (var ix = 0; ix < enemySegment.Blocks.GetLength(0); ix++)
            {
                if (blocks[ix, iy] == null) continue;

                var tempDistance = SegmentToSegmentDistance((x, y), (ix, iy), enemyDirection);
                if (distance != null && !(tempDistance < distance)) continue;

                distance = tempDistance;
                target = (ix, iy);
            }

            return target;
        }

        private static int SegmentToSegmentDistance(
            (int x, int y) attackerLocation,
            (int x, int y) defenderLocation,
            Direction enemyDirection)
        {
            var defenderLocationAdjusted = defenderLocation;
            switch (enemyDirection)
            {
                case Direction.Up:
                    defenderLocationAdjusted.y += 9;
                    break;
                case Direction.Down:
                    defenderLocationAdjusted.y -= 9;
                    break;
                case Direction.Left:
                    defenderLocationAdjusted.x -= 9;
                    break;
                case Direction.Right:
                    defenderLocationAdjusted.x += 9;
                    break;
            }

            return (int)Math.Pow(
                Math.Pow(attackerLocation.x - defenderLocationAdjusted.x, 2) +
                Math.Pow(attackerLocation.y - defenderLocationAdjusted.y, 2), 0.5);
        }
    }
}
