using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCapture.Abstractions.Events
{
    public delegate void ErroredEventHandler(ErroredEventArgs e);

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
}
