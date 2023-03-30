using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Hairdresser
{
    public class BarberShop
    {
        private readonly WaitingBench _waitingBench;

        private readonly BarberShopManager _manager;

        private readonly ConcurrentQueue<Customer> _newCustomers;

        private readonly Barbers _barbers;

        private readonly AutoResetEvent _waitingManagerEvent;

        private readonly AutoResetEvent _newCustomersManagerEvent;

        private readonly AutoResetEvent _customerEvent;

        public BarberShop(int capacity, int barbersNumber)
        {
            if (capacity <= 0)
                throw new ArgumentException(nameof(capacity));
            if (barbersNumber <= 0)
                throw new ArgumentException(nameof(barbersNumber));

            _customerEvent = new AutoResetEvent(false);
            _waitingManagerEvent = new AutoResetEvent(false);
            _newCustomersManagerEvent = new AutoResetEvent(false);
            _newCustomers = new ConcurrentQueue<Customer>();
            _waitingBench = new WaitingBench(capacity);
            _barbers = new Barbers(barbersNumber, _waitingManagerEvent);
            _manager = new BarberShopManager(_waitingBench, _newCustomers, _barbers, _waitingManagerEvent, _newCustomersManagerEvent, _customerEvent);
        }

        public void Open()
        {
            _manager.Start();
            _barbers.Start();
        }

        public void HandleNewCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _customerEvent.WaitOne();
            lock(_newCustomers)
            {
                _newCustomersManagerEvent.Set();
                _newCustomers.Enqueue(customer);
            }
        }
    }
}