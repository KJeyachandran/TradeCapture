using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCapture.Foundation.Events
{
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
