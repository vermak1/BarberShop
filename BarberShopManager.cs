using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Hairdresser
{
    internal class BarberShopManager
    {
        private readonly WaitingBench _bench;

        private readonly Barbers _barbers;

        private readonly Thread _processNewCustomersThread;

        private readonly Thread _processWaitingCustomersThread;

        private readonly ConcurrentQueue<Customer> _newCustomers;

        private readonly AutoResetEvent _waitingManagerEvent;

        private readonly AutoResetEvent _newCustomerEvent;

        private readonly AutoResetEvent _customerEvent;

        public BarberShopManager(WaitingBench bench, ConcurrentQueue<Customer> customers, Barbers barbers, AutoResetEvent waitingEvent, AutoResetEvent newCustomerManagerEvent, AutoResetEvent customerEvent)
        {
            _bench = bench;
            _newCustomers = customers;
            _barbers = barbers;
            _waitingManagerEvent = waitingEvent;
            _newCustomerEvent = newCustomerManagerEvent;
            _customerEvent = customerEvent;

            _processNewCustomersThread = new Thread(() =>
            {
                StartProcessNewCustomers();
            });

            _processWaitingCustomersThread = new Thread(() =>
            {
                StartProcessWaitingCustomers();
            });
        }

        private void AddCustomerToChair(Customer customer, Barber barber)
        {
            Console.WriteLine("[Manager] Add customer {0} to the chair of barber {1}", customer.Name, barber.Number);
            barber.BarberChair.Customer = customer;
            barber.ChairEvent.Set();
        }

        private void AddCustomerToBench(Customer customer)
        {
            _bench.AddCustomerToBench(customer);
            Console.WriteLine("[Manager] Add customer {0} to the bench", customer.Name);
        }

        private void ProcessNewCustomer()
        {
            Customer customer = null;
            _customerEvent.Set();
            _newCustomerEvent.WaitOne();
            lock(_newCustomers)
            {
                if (!_newCustomers.TryDequeue(out customer))
                    return;
            }

            lock (_bench)
            {
                var barber = _barbers.GetBarberIfReadyForWork();
                if (barber != null && !_bench.SomeoneOnBench)
                {
                    AddCustomerToChair(customer, barber);
                    return;
                }
                if (_bench.Available)
                {
                    AddCustomerToBench(customer);
                    return;
                }
            }
            Console.WriteLine("Capacity is full, customer #{0} has gone", customer.Name);
            
        }

        private void StartProcessNewCustomers()
        {
            try
            {
                while (true)
                {
                    ProcessNewCustomer();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Manager failed within " + nameof(StartProcessNewCustomers));
                Console.WriteLine(ex.Message);
            }
        }

        private void StartProcessWaitingCustomers()
        {
            try
            {
                while(true)
                {
                    ProcessWaitingCustomer();
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Manager failed within " + nameof(StartProcessWaitingCustomers));
                Console.WriteLine(ex.Message);
            }
        }

        private void ProcessWaitingCustomer()
        {
            _waitingManagerEvent.WaitOne();
            lock (_bench) 
            {
                var barber = _barbers.GetBarberIfReadyForWork();
                Customer customer = null;
                if (barber == null)
                    return;
                if (!_bench.TryRemoveCustomerFromBench(out customer))
                {
                    return;
                }
                AddCustomerToChair(customer, barber);
            }
        }

        public void Start()
        {
            _processNewCustomersThread.Start();
            _processWaitingCustomersThread.Start();
        }
    }
}
