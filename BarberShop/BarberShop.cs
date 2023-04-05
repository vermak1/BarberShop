using System;
using System.Threading;

namespace Hairdresser
{
    public class BarberShop : IDisposable
    {
        private readonly BarberShopManager _manager;

        private readonly NewCustomers _newCustomers;

        private readonly Barbers _barbers;

        private readonly AutoResetEvent _benchProcessorEvent;

        private readonly AutoResetEvent _newCustomerProcessorEvent;

        private readonly AutoResetEvent _customerEvent;

        private readonly AutoResetEvent _waitingCustomerProcessorEvent;

        private readonly AutoResetEvent _streetCustomerProcessorEvent;

        public BarberShop(int benchCapacity, int barbersNumber)
        {
            try
            {
                _customerEvent = new AutoResetEvent(false);
                _benchProcessorEvent = new AutoResetEvent(false);
                _newCustomerProcessorEvent = new AutoResetEvent(false);
                _waitingCustomerProcessorEvent = new AutoResetEvent(false);
                _streetCustomerProcessorEvent= new AutoResetEvent(false);

                _newCustomers = new NewCustomers();
                WaitingBench waitingBench = new WaitingBench(benchCapacity);
                _barbers = new Barbers(barbersNumber, _benchProcessorEvent);
                _manager = new BarberShopManager(waitingBench, _newCustomers, _barbers, _benchProcessorEvent, _newCustomerProcessorEvent, _customerEvent, _waitingCustomerProcessorEvent, _streetCustomerProcessorEvent);

                _manager.Start();
                _barbers.Start();
            }
            catch (Exception ex)
            {
                Dispose();
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }
        public void Dispose()
        {
            _benchProcessorEvent?.Dispose();
            _newCustomerProcessorEvent?.Dispose();
            _customerEvent?.Dispose();
            _waitingCustomerProcessorEvent?.Dispose();
            _streetCustomerProcessorEvent?.Dispose();
        }

        public void HandleNewCustomer(Customer customer)
        {
            if (customer == null)
                throw new ArgumentNullException(nameof(customer));

            _customerEvent.WaitOne();
            lock(_newCustomers)
            {
                _newCustomerProcessorEvent.Set();
                _newCustomers.Enqueue(customer);
            }
        }

    }
}