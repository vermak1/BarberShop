using System;
using System.Threading;

namespace Hairdresser
{
    internal class StreetCustomerProcessor
    {
        private static String PROCESSOR_NAME = nameof(StreetCustomerProcessor);

        private readonly StreetQueue _streetQueue;

        private readonly WaitingBench _bench;

        private readonly AutoResetEvent _streetCustomerProcessorEvent;

        private readonly AutoResetEvent _benchProcessorEvent;

        private readonly Thread _streetCustomerThread;

        public StreetCustomerProcessor(StreetQueue streetQueue, WaitingBench bench, AutoResetEvent streetCustomerProcessorEvent, AutoResetEvent benchProcessorEvent)
        {
            _streetQueue = streetQueue;
            _bench = bench;
            _streetCustomerProcessorEvent = streetCustomerProcessorEvent;
            _benchProcessorEvent = benchProcessorEvent;

            _streetCustomerThread = new Thread(() =>
            {
                try
                {
                    StartProcessStreetCustomers();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Processor failed within " + nameof(StartProcessStreetCustomers));
                    Console.WriteLine(ex.Message);
                    Console.WriteLine(ex.StackTrace);
                }
            });
        }

        public void StartWork()
        {
            _streetCustomerThread.Start();
        }

        private void StartProcessStreetCustomers()
        {
            while(true) 
            {
                ProcessStreetCustomer();
            }
        }

        private void ProcessStreetCustomer()
        {
            _streetCustomerProcessorEvent.WaitOne(1000);
            _streetQueue.TryPeek(out Customer customer);
            if (customer == null)
                return;

            lock (_bench)
            {
                lock (_streetQueue)
                {
                    if (_bench.Available)
                    {
                        AddCustomerToBench(customer);
                        _streetQueue.TryDequeue(out _);
                    }
                }
            }
        }

        private void AddCustomerToBench(Customer customer)
        {
            _benchProcessorEvent.Set();
            _bench.Enqueue(customer);
            Console.WriteLine("[{0}] Add customer {1} to the Bench", PROCESSOR_NAME, customer.Name);
        }
    }
}
