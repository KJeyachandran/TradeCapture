namespace TradeCapture.Foundation.Events;

public class PlacedEventArgs
{
    public PlacedEventArgs(string code, decimal price)
    {
        Code = code;
        Price = price;
    }

    public string Code { get; }

    public decimal Price { get; }
}
