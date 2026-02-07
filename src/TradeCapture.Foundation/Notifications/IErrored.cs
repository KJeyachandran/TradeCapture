using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TradeCapture.Foundation.Events;

namespace TradeCapture.Foundation.Notifications
{
    public delegate void ErroredEventHandler(ErroredEventArgs e);

    public interface IErrored
    {
        event ErroredEventHandler Errored;
    }
}
