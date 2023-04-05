﻿using System;
using System.Threading;

namespace Hairdresser
{
    internal class WaitingCustomerProcessor
    {
        private static String PROCESSOR_NAME = nameof(WaitingCustomerProcessor);

        private readonly WaitingCustomers _waitingCustomers;

        private readonly StreetQueue _streetQueue;

        private readonly WaitingBench _bench;

        private readonly AutoResetEvent _waitingCustomerProcessorEvent;

        private readonly AutoResetEvent _streetCustomerProcessorEvent;

        private readonly Thread _waitingCustomerThread;

        public WaitingCustomerProcessor(StreetQueue streetQueue, WaitingCustomers waitingCustomers, WaitingBench bench, AutoResetEvent waitingCustomerProcessorEvent, AutoResetEvent streetCustomerProcessorEvent)
        {
            _waitingCustomers = waitingCustomers;
            _streetQueue = streetQueue;
            _bench = bench;
            _waitingCustomerProcessorEvent = waitingCustomerProcessorEvent;
            _streetCustomerProcessorEvent= streetCustomerProcessorEvent;

            _waitingCustomerThread = new Thread(() =>
            {
                try
                {
                    StartProcessWaitingCustomers();
                }
                catch (Exception ex) 
                {
                    Console.WriteLine("Processor failed within " + nameof(StartProcessWaitingCustomers));
                    Console.WriteLine(ex.Message);
                }
            });
        }

        public void StartWork()
        {
            _waitingCustomerThread.Start();
        }

        private void StartProcessWaitingCustomers()
        {
            while (true)
            {
                ProcessWaitingCustomer();
            }
        }

        private void ProcessWaitingCustomer()
        {
            _waitingCustomerProcessorEvent.WaitOne(1000);
            Customer customer;
            while (_waitingCustomers.TryPeek(out customer) && customer.WaitedForTime)
            {
                if (!customer.IsHere)
                {
                    Console.WriteLine("[{0}] Customer {1} has gone due to work call", PROCESSOR_NAME, customer.Name);
                    _waitingCustomers.TryDequeue(out _);
                    continue;
                }

                lock (_bench)
                {
                    if (_bench.Available && !_streetQueue.SomeoneOnStreet)
                    {
                        Console.WriteLine("[{0}] Customer {1} added to bench since street and bench are empty", PROCESSOR_NAME, customer.Name);
                        AddCustomerToBench(customer);
                        _waitingCustomers.TryDequeue(out _);
                        continue;
                    }
                }

                if (!customer.IsAgreedToWaitOnStreet)
                {
                    Console.WriteLine("[{0}] Customer {1} is not agreed to wait on street, so he is gone", PROCESSOR_NAME, customer.Name);
                    _waitingCustomers.TryDequeue(out _);
                    continue;
                }
                AddCustomerToStreet(customer);
                _waitingCustomers.TryDequeue(out _);
            }
        }

        private void AddCustomerToBench(Customer customer)
        {
            _bench.Enqueue(customer);
            Console.WriteLine("[{0}] Add customer {1} to the Bench", PROCESSOR_NAME, customer.Name);
        }

        private void AddCustomerToStreet(Customer customer)
        {
            _streetQueue.Enqueue(customer);
            _streetCustomerProcessorEvent.Set();
            Console.WriteLine("[{0}] Add customer {1} to the Street", PROCESSOR_NAME, customer.Name);
        }
    }
}
