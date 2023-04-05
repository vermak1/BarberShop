using System;
using System.Collections.Concurrent;

namespace Hairdresser
{
    internal class WaitingBench : AbstractQueue
    {

        private readonly Int32 _capacity;

        public Boolean Available => _queue.Count < _capacity;

        public Boolean SomeoneOnBench => _queue.Count > 0;

        public WaitingBench(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException(nameof(capacity));

            _capacity = capacity;
        }
    }
}
