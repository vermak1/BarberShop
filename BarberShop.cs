using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Hairdresser
{
    public class BarberShop
    {
        private readonly WaitingBench _waitingBench;

        private readonly HairCutter _hairCutter;

        private readonly Chair _chair;

        private readonly BarberShopManager _manager;

        private readonly AutoResetEvent _chairEvent;

        private readonly ConcurrentQueue<Customer> _newCustomers;

        private readonly AutoResetEvent _waitingManagerEvent;

        private readonly Object _chairSyncRoot = new object();

        public BarberShop(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException(nameof(capacity));

            _newCustomers = new ConcurrentQueue<Customer>();
            _chairEvent = new AutoResetEvent(false);
            _waitingManagerEvent = new AutoResetEvent(false);
            _waitingBench = new WaitingBench(capacity);
            _chair = new Chair(_chairEvent);
            _hairCutter = new HairCutter(_chair, _chairSyncRoot, _chairEvent, _waitingManagerEvent);
            _manager = new BarberShopManager(_waitingBench, _chairSyncRoot, _chairEvent, _chair, _newCustomers, _waitingManagerEvent);
        }

        public void Open()
        {
            _manager.Start();
            _hairCutter.Start();
        }

        public void HandleNewCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            lock(_newCustomers)
                _newCustomers.Enqueue(customer);
        }
    }
}