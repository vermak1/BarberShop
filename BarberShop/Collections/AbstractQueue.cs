using System;
using System.Collections.Concurrent;

namespace Hairdresser
{
    internal abstract class AbstractQueue
    {
        protected readonly ConcurrentQueue<Customer> _queue;

        public AbstractQueue()
        {
            _queue = new ConcurrentQueue<Customer>();
        }

        public virtual Boolean TryDequeue(out Customer customer)
        {
            return _queue.TryDequeue(out customer);
        }

        public virtual void Enqueue(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _queue.Enqueue(customer);
        }

        public virtual Boolean TryPeek(out Customer customer)
        {
            return _queue.TryPeek(out customer);
        }
    }
}
