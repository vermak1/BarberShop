using System.Threading;

namespace Hairdresser
{
    internal class BarberShopManager
    {
        private readonly NewCustomerProcessor _newCustomerProcessor;

        private readonly BenchCustomerProcessor _benchCustomerProcessor;

        private readonly WaitingCustomerProcessor _waitingCustomerProcessor;

        private readonly StreetCustomerProcessor _streetCustomerProcessor;

        public BarberShopManager(WaitingBench bench, NewCustomers newCustomers, Barbers barbers, AutoResetEvent benchProcessorEvent, AutoResetEvent newCustomerProcessorEvent, AutoResetEvent customerEvent, AutoResetEvent waitingCustomerProcessorEvent, AutoResetEvent streetCustomerProcessorEvent)
        {
            WaitingCustomers waitingCustomers = new WaitingCustomers();
            StreetQueue streetQueue = new StreetQueue();

            _newCustomerProcessor = new NewCustomerProcessor(newCustomers, newCustomerProcessorEvent, customerEvent, barbers, bench, waitingCustomers, waitingCustomerProcessorEvent, streetQueue);
            _benchCustomerProcessor = new BenchCustomerProcessor(benchProcessorEvent, bench, barbers);
            _waitingCustomerProcessor = new WaitingCustomerProcessor(streetQueue, waitingCustomers, bench, waitingCustomerProcessorEvent, streetCustomerProcessorEvent);
            _streetCustomerProcessor = new StreetCustomerProcessor(streetQueue, bench, streetCustomerProcessorEvent);
        }

        public void Start()
        {
            _newCustomerProcessor.StartWork();
            _benchCustomerProcessor.StartWork();
            _waitingCustomerProcessor.StartWork();
            _streetCustomerProcessor.StartWork();
        }
    }
}
