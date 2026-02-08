using TradeCapture.Foundation.Events;
using TradeCapture.Foundation.Notifications;
using TradeCapture.Foundation.Orders;

public sealed class Order : IOrder
{
    private const int DEFAULT_QUANTITY = 100;       // set a default quantity

    private readonly object _locker = new object(); // for thread sync

    private readonly IOrderService _orderService;

    private readonly decimal _threshold;
    private volatile bool _hasPlacedOrder = false;
    private volatile bool _hasErrored = false;

    public event PlacedEventHandler? Placed;
    public event ErroredEventHandler? Errored;

    public Order(IOrderService orderService, decimal threshold)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _threshold = threshold;
    }

    public void RespondToTick(string code, decimal price)
    {
        // fast checks without lock
        if (string.IsNullOrWhiteSpace(code) || _hasPlacedOrder || _hasErrored)
        {
            return;
        }

        lock (_locker)
        {
            // double-check again for thread safety
            if (_hasPlacedOrder || _hasErrored)
            {
                return;
            }

            try
            {
                if (price < _threshold)
                {
                    _orderService.Buy(code, DEFAULT_QUANTITY, price);
                    _hasPlacedOrder = true;
                    OnPlaced(new PlacedEventArgs(code, price));
                }
            }
            catch (Exception ex)
            {
                _hasErrored = true;
                OnErrored(new ErroredEventArgs(code, price, ex));
            }
        }
    }

    public void OnPlaced(PlacedEventArgs e)
    {
        Placed?.Invoke(e);
    }

    public void OnErrored(ErroredEventArgs e)
    {
        Errored?.Invoke(e);
    }
}
