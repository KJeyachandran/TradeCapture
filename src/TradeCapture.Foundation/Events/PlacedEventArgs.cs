using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCapture.Abstractions.Events
{
    public delegate void PlacedEventHandler(PlacedEventArgs e);

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
}
