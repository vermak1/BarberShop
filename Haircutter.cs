using System;
using System.Threading;

namespace Hairdresser
{
    public class HairCutter
    {
        private readonly Random _random;

        private readonly Thread _workingThread;

        private readonly Chair _chair;

        private readonly Object _chairSyncRoot;

        private readonly AutoResetEvent _chairEvent;

        private readonly AutoResetEvent _managerEvent;

        public HairCutter(Chair chair, Object syncRoot, AutoResetEvent chairEvent, AutoResetEvent managerEvent)
        {
            _chairSyncRoot = syncRoot;
            _chair = chair;
            _random = new Random();
            _chairEvent = chairEvent;
            _managerEvent = managerEvent;
            _workingThread = new Thread(() =>
            {
                StartWaitForHaircut();
            });
        }

        private void StartHairCut()
        {
            TimeSpan timeForHaircut = TimeSpan.FromSeconds(_random.Next(2, 5));
            Console.WriteLine("Started haircut for customer #{0}, duration {1} seconds", _chair.Customer.Name, timeForHaircut.Seconds);
            Thread.Sleep(timeForHaircut);
            Console.WriteLine("Finished haircut for customer #{0}", _chair.Customer.Name);
        }

        private void FollowToDoor()
        {
            TimeSpan farewell = _chair.Customer.FarewellTime;
            Console.WriteLine("Following customer {0} to the door, time: {1} sec", _chair.Customer.Name, farewell.TotalSeconds);
            lock (_chairSyncRoot)
                _chair.Customer = null;

            _managerEvent.Set();
            Thread.Sleep(farewell);
        }

        private void StartWaitForHaircut()
        {
            try
            {
                while (true)
                {
                    _chairEvent.WaitOne();
                    lock (_chairSyncRoot)
                    {
                        StartHairCut();
                    }
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
