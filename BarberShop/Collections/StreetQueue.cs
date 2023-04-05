using System;

namespace Hairdresser
{
    internal class StreetQueue : AbstractQueue
    {
        public Boolean SomeoneOnStreet => _queue.Count > 0;
    }
}
