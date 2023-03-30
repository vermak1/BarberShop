using System;
using System.Threading;

namespace Hairdresser
{
    public class Chair
    {
        public Customer Customer { get; set; }

        private readonly AutoResetEvent _event;
        public Chair(AutoResetEvent chairEvent)
        {
            _event = chairEvent;
        }
    }
}
