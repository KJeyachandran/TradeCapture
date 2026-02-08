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

    /// <summary>Occurs when an order has been successfully placed.</summary>
    public event PlacedEventHandler? Placed;

    /// <summary>Occurs when an error occurs while placing an order.</summary>
    public event ErroredEventHandler? Errored;

    /// <summary>
    /// Creates andi initializes a new instance of the <see cref="Order"/> class.i
    /// </summary>
    /// <param name="orderService">The service used to execute orders.</param>
    /// <param name="threshold">The price threshold at which to place a buy order.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="orderService"/> is null.
    public Order(IOrderService orderService, decimal threshold)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _threshold = threshold;
    }

    /// <summary>
    /// Responds to a price tick and places an order if the price falls below the threshold.
    /// </summary>
    /// <param name="code">The security code (e.g., stock symbol).</param>
    /// <param name="price">The current price of the security.</param>
    /// <remarks>
    /// This method is thread-safe and will only place an order once per instance.
    /// If the price is below the threshold, a buy order for the default quantity is placed.
    /// </remarks>
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

    /// <summary>
    /// Raises the <see cref="Placed"/> event.
    /// </summary>
    /// <param name="e">The event arguments containing order placement details.</param>
    public void OnPlaced(PlacedEventArgs e)
    {
        Placed?.Invoke(e);
    }

    /// <summary>
    /// Raises the <see cref="Errored"/> event.
    /// </summary>
    /// <param name="e">The event arguments containing error details.</param>
    public void OnErrored(ErroredEventArgs e)
    {
        Errored?.Invoke(e);
    }
}
