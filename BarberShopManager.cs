using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Hairdresser
{
    internal class BarberShopManager
    {
        private readonly Object _chairSyncRoot;

        private readonly WaitingBench _bench;

        private readonly AutoResetEvent _chairEvent;

        private readonly Chair _chair;

        private readonly Thread _processNewCustomersThread;

        private readonly Thread _processWaitingCustomersThread;

        private readonly ConcurrentQueue<Customer> _newCustomers;

        private readonly AutoResetEvent _waitingManagerEvent;

        public BarberShopManager(WaitingBench bench, Object syncRoot, AutoResetEvent chairEvent, Chair chair, ConcurrentQueue<Customer> customers, AutoResetEvent waitingmanagerEvent)
        {
            _bench = bench;
            _chairSyncRoot = syncRoot;
            _chairEvent = chairEvent;
            _chair = chair;
            _newCustomers = customers;
            _waitingManagerEvent = waitingmanagerEvent;

            _processNewCustomersThread = new Thread(() =>
            {
                StartProcessNewCustomers();
            });

            _processWaitingCustomersThread = new Thread(() =>
            {
                StartProcessWaitingCustomers();
            });
        }

        private void AddCustomerToChair(Customer customer)
        {
            Console.WriteLine("[Manager] Add customer {0} to the chair", customer.Name);
            _chair.Customer = customer;
            _chairEvent.Set();
        }

        private void AddCustomerToBench(Customer customer)
        {
            _bench.AddCustomerToBench(customer);
            Console.WriteLine("[Manager] Add customer {0} to the bench. Current Count: {1}", customer.Name, _bench.Count);
        }

        private void ProcessNewCustomer()
        {
            Customer customer = null;
            if (!_newCustomers.TryDequeue(out customer))
                return;

            lock (_bench)
            {
                if (_chair.Customer == null && !_bench.SomeoneOnBench)
                {
                    AddCustomerToChair(customer);
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
            Customer customer = null;
            if (!_bench.TryRemoveCustomerFromBench(out customer))
            {
                return;
            }
            
            lock (_chairSyncRoot)
            {
                AddCustomerToChair(customer);
            }

        }

        public void Start()
        {
            _processNewCustomersThread.Start();
            _processWaitingCustomersThread.Start();
        }
    }
}
