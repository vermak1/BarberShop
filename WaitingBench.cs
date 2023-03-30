using System;
using System.Collections.Concurrent;

namespace Hairdresser
{
    internal class WaitingBench
    {
        private readonly ConcurrentQueue<Customer> _waitingQueue;

        private readonly Int32 _capacity;

        public Boolean Available => _waitingQueue.Count < _capacity;

        public Boolean SomeoneOnBench => _waitingQueue.Count > 0;

        public Int32 Count => _waitingQueue.Count;

        public WaitingBench(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException(nameof(capacity));

            _capacity = capacity;
            _waitingQueue = new ConcurrentQueue<Customer>();
        }

        public void AddCustomerToBench(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _waitingQueue.Enqueue(customer);
        }

        public Boolean TryRemoveCustomerFromBench(out Customer customer)
        {
            return _waitingQueue.TryDequeue(out customer);
        }
    }
}
