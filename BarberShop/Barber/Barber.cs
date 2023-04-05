using System;
using System.Threading;

namespace Hairdresser
{
    public class Barber
    {
        private class Chair
        {
            public Customer Customer { get; set; }
        }

        public Int32 Number { get; }

        private readonly Random _random;

        private readonly Thread _workingThread;

        private readonly AutoResetEvent _waitingManagerEvent;

        private readonly Chair _chair;

        private readonly AutoResetEvent _chairEvent;

        public Boolean IsBarberFree => _chair.Customer == null;

        public Barber(int number, AutoResetEvent waitingManagerEvent)
        {
            Number = number;
            _chair = new Chair();
            _random = new Random();
            _chairEvent = new AutoResetEvent(false);
            _waitingManagerEvent = waitingManagerEvent;
            _workingThread = new Thread(() =>
            {
                StartWaitForHaircut();
            });
        }

        private void StartHairCut()
        {
            TimeSpan timeForHaircut = TimeSpan.FromSeconds(_random.Next(2, 5));
            Console.WriteLine("Started haircut for customer #{0}, duration {1} seconds, barber #{2}", _chair.Customer.Name, timeForHaircut.Seconds, Number);
            Thread.Sleep(timeForHaircut);
            Console.WriteLine("Finished haircut for customer #{0}, barber #{1}", _chair.Customer.Name, Number);
        }

        private void FollowToDoor()
        {
            TimeSpan farewell = _chair.Customer.FarewellTime;
            Console.WriteLine("Following customer {0} to the door, time: {1} sec, barber #{2}", _chair.Customer.Name, farewell.TotalSeconds, Number);
            _chair.Customer = null;
            _waitingManagerEvent.Set();
            Thread.Sleep(farewell);
        }

        private void StartWaitForHaircut()
        {
            try
            {
                while (true)
                {
                    _chairEvent.WaitOne();
                    StartHairCut();
                    FollowToDoor();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Haircutter failed within " + nameof(StartWaitForHaircut));
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        public void AddCustomerToChair(Customer customer)
        {
            Console.WriteLine("Add customer {0} to the chair of barber {1}", customer.Name, Number);
            _chair.Customer = customer;
            _chairEvent.Set();
        }

        public void Start()
        {
            _workingThread.Start();
        }
    }
}
