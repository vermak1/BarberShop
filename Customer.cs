using System;

namespace Hairdresser
{
    public class Customer
    {
        private readonly Random _random;
        public Customer()
        {
            _random = new Random();
        }

        public Boolean Answer => _random.Next(1, 100) % 2 == 0;
           
        public TimeSpan FarewellTime => TimeSpan.FromSeconds(_random.Next(1,5));
        public String Name { get; set; }

    }
}
