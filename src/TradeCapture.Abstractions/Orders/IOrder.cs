using System;
using TradeCapture.Abstractions.Events;

namespace TradeCapture.Abstractions.Orders
{
    public interface IOrder : IPlaced, IErrored
    {
        void RespondToTick(string code, decimal price);
    }
}
