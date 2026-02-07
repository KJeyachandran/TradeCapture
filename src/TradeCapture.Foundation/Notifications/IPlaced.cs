using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeCapture.Foundation.Events;

namespace TradeCapture.Foundation.Notifications
{
    public delegate void PlacedEventHandler(PlacedEventArgs e);

    public interface IPlaced
    {
        event PlacedEventHandler Placed;
    }
}
