using System;
using System.Threading;

namespace Hairdresser
{
    public class Customer
    {
        private class Employer
        {
            private readonly CancellationTokenSource _cancellationTokenSource;

            public CancellationToken Token => _cancellationTokenSource.Token;

            private readonly Random _random;

            public Employer()
            {
                _random = new Random();
                _cancellationTokenSource = new CancellationTokenSource();
                ThreadPool.QueueUserWorkItem((a) =>
                {
                    MakeRandomCall();
                });
            }

            private void MakeRandomCall()
            {
                Thread.Sleep(TimeSpan.FromSeconds(_random.Next(0, 30)));
                _cancellationTokenSource.Cancel();
            }
        }

        public String Name { get; set; }

        private readonly Random _random;

        private readonly Employer _employer;

        public Boolean IsAgreedToWaitOnStreet => _random.Next(0, 100) % 2 == 0;

        public TimeSpan FarewellTime => TimeSpan.FromSeconds(_random.Next(1, 5));

        public Boolean IsHere { get; private set; }

        public Boolean WaitedForTime { get; private set; }

        public Customer()
        {
            IsHere = true;
            _random = new Random();
            _employer = new Employer();
            _employer.Token.Register(() =>
            {
                IsHere = false;
            });
        }

        public void Wait(TimeSpan timeToWait)
        {
            ThreadPool.QueueUserWorkItem((a) => 
            {
                Thread.Sleep(timeToWait);
                WaitedForTime = true;
            });
        }
    }
}
