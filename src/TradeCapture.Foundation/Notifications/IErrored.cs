using TradeCapture.Foundation.Events;

namespace TradeCapture.Foundation.Notifications;

public delegate void ErroredEventHandler(ErroredEventArgs e);

public interface IErrored
{
    event ErroredEventHandler Errored;
}
