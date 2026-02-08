using TradeCapture.Foundation.Notifications;

namespace TradeCapture.Foundation.Orders;

public interface IOrder : IPlaced, IErrored
{
    void RespondToTick(string code, decimal price);
}
