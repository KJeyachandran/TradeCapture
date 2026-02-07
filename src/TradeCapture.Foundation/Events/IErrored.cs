using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TradeCapture.Abstractions.Events
{
    public interface IErrored
    {
        event ErroredEventHandler Errored;
    }
}
