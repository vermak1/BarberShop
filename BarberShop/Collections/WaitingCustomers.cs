using System;

namespace Hairdresser
{
    internal class WaitingCustomers : AbstractQueue
    {
        public Boolean SomeoneInWaitingQueue => _queue.Count > 0;
    }
}
