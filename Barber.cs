using System;
using System.Threading;

namespace Hairdresser
{
    public class Barber
    {
        public Int32 Number { get; }

        private readonly Random _random;

        private readonly Thread _workingThread;

        private readonly AutoResetEvent _waitingManagerEvent;

        public Chair BarberChair { get; }

        public AutoResetEvent ChairEvent { get; }

        public Barber(int number, AutoResetEvent waitingManagerEvent)
        {
            Number = number;
            BarberChair = new Chair();
            _random = new Random();
            ChairEvent = new AutoResetEvent(false);
            _waitingManagerEvent = waitingManagerEvent;
            _workingThread = new Thread(() =>
            {
                StartWaitForHaircut();
            });
        }

        private void StartHairCut()
        {
            TimeSpan timeForHaircut = TimeSpan.FromSeconds(_random.Next(2, 5));
            Console.WriteLine("Started haircut for customer #{0}, duration {1} seconds, barber #{2}", BarberChair.Customer.Name, timeForHaircut.Seconds, Number);
            Thread.Sleep(timeForHaircut);
            Console.WriteLine("Finished haircut for customer #{0}, barber #{1}", BarberChair.Customer.Name, Number);
        }

        private void FollowToDoor()
        {
            TimeSpan farewell = BarberChair.Customer.FarewellTime;
            Console.WriteLine("Following customer {0} to the door, time: {1} sec, barber #{2}", BarberChair.Customer.Name, farewell.TotalSeconds, Number);
            BarberChair.Customer = null;
            Thread.Sleep(farewell);
        }

        private void StartWaitForHaircut()
        {
            try
            {
                while (true)
                {
                    ChairEvent.WaitOne();
                    StartHairCut();
                    _waitingManagerEvent.Set();
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

        public void Start()
        {
            _workingThread.Start();
        }
    }
}
