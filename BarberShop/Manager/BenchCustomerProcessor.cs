using System;
using System.Threading;

namespace Hairdresser
{
    internal class BenchCustomerProcessor
    {
        private readonly Thread _processBenchCustomersThread;

        private readonly WaitingBench _bench;

        private readonly AutoResetEvent _benchProcessorEvent;

        private readonly Barbers _barbers;

        public BenchCustomerProcessor(AutoResetEvent benchProcessorEvent, WaitingBench bench, Barbers barbers)
        {
            _bench = bench;
            _benchProcessorEvent = benchProcessorEvent;
            _barbers = barbers;

            _processBenchCustomersThread = new Thread(() =>
            {
                try
                {
                    StartProcessBenchCustomers();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Processor failed within " + nameof(StartProcessBenchCustomers));
                    Console.WriteLine(ex.Message);
                }
            });
        }

        private void StartProcessBenchCustomers()
        {
            while (true)
            {
                ProcessBenchCustomer();
            }
        }

        private void ProcessBenchCustomer()
        {
            _benchProcessorEvent.WaitOne();
            lock (_bench)
            {
                var barber = _barbers.GetBarberIfReadyForWork();
                while (barber != null)
                {
                    if (!_bench.TryDequeue(out Customer customer))
                        return;
                    barber.AddCustomerToChair(customer);
                    barber = _barbers.GetBarberIfReadyForWork();
                }
            }
        }

        public void StartWork()
        {
            _processBenchCustomersThread.Start();
        }
    }
}
