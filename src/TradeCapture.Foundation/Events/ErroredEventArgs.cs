namespace TradeCapture.Foundation.Events;

public class ErroredEventArgs : ErrorEventArgs
{
    public ErroredEventArgs(string code, decimal price, Exception ex) : base(ex)
    {
        Code = code;
        Price = price;
    }

    public string Code { get; }

    public decimal Price { get; }
}
