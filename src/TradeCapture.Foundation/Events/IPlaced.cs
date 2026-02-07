using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCapture.Abstractions.Events
{
    public interface IPlaced
    {
        event PlacedEventHandler Placed;
    }
}
