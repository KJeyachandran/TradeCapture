using TradeCapture.Foundation.Events;

namespace TradeCapture.Foundation.Notifications;

public delegate void PlacedEventHandler(PlacedEventArgs e);

public interface IPlaced
{
    event PlacedEventHandler Placed;
}
