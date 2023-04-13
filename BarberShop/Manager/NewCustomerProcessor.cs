using System;
using System.Threading;

namespace Hairdresser
{
    internal class NewCustomerProcessor
    {
        private static String PROCESSOR_NAME = nameof(NewCustomerProcessor);

        private readonly NewCustomers _newCustomers;

        private readonly WaitingBench _bench;

        private readonly WaitingCustomers _waitingCustomers;

        private readonly StreetQueue _streetQueue;

        private readonly Barbers _barbers;

        private readonly AutoResetEvent _newCustomerProcessorEvent;

        private readonly AutoResetEvent _customerEvent;

        private readonly AutoResetEvent _waitingProcessorEvent;

        private readonly AutoResetEvent _benchProcessorEvent;

        private readonly Thread _processNewCustomersThread;

        private readonly TimeSpan _timeToWaitForCustomer = TimeSpan.FromSeconds(15);

        public NewCustomerProcessor(NewCustomers newCustomers, AutoResetEvent newCustomerProcessorEvent, AutoResetEvent customerEvent, Barbers barbers, 
                                    WaitingBench bench, WaitingCustomers waitingCustomers, AutoResetEvent waitingCustomerProcessorEvent, StreetQueue streetQueue, 
                                    AutoResetEvent benchProcessorEvent)
        {
            _newCustomers = newCustomers;
            _newCustomerProcessorEvent = newCustomerProcessorEvent;
            _customerEvent = customerEvent;
            _barbers = barbers;
            _bench = bench;
            _waitingCustomers = waitingCustomers;
            _waitingProcessorEvent = waitingCustomerProcessorEvent;
            _streetQueue = streetQueue;
            _benchProcessorEvent = benchProcessorEvent;

            _processNewCustomersThread = new Thread(() =>
            {
                try
                {
                    StartProcessNewCustomers();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Processor failed within " + nameof(StartProcessNewCustomers));
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public void StartWork()
        {
            _processNewCustomersThread.Start();
        }

        private void StartProcessNewCustomers()
        {
            while (true)
            {
                ProcessNewCustomer();
            }
        }

        private void ProcessNewCustomer()
        {
            Customer customer;
            _customerEvent.Set();
            _newCustomerProcessorEvent.WaitOne();
            if (!_newCustomers.TryDequeue(out customer))
                return;
            

            if (_streetQueue.SomeoneOnStreet || _waitingCustomers.SomeoneInWaitingQueue)
            {
                AddCustomerToWaitingQueue(customer);
                return;
            }

            var barber = _barbers.GetBarberIfReadyForWork();
            lock (_bench)
            {
                if (barber != null && !_bench.SomeoneOnBench)
                {
                    barber.AddCustomerToChair(customer);
                    return;
                }

                if (_bench.Available)
                {
                    AddCustomerToBench(customer);
                    return;
                }
            }
            AddCustomerToWaitingQueue(customer);
        }

        private void AddCustomerToBench(Customer customer)
        {
            _benchProcessorEvent.Set();
            _bench.Enqueue(customer);
            Console.WriteLine("[{0}] Add customer {1} to the Bench", PROCESSOR_NAME, customer.Name);
        }

        private void AddCustomerToWaitingQueue(Customer customer)
        {
            Console.WriteLine("[{0}] Add customer {1} to the WaitingQueue", PROCESSOR_NAME, customer.Name);
            customer.Wait(_timeToWaitForCustomer);
            _waitingCustomers.Enqueue(customer);
            _waitingProcessorEvent.Set();
        }
    }
}
