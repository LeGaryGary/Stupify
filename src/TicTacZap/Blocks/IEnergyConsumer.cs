using System;
using System.Collections.Generic;
using System.Text;

namespace TicTacZap.Blocks
{
    public interface IEnergyConsumer : IBlock
    {
        int EnergyConsumption { get; }
    }
}
