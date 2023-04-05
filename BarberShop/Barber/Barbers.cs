using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Hairdresser
{
    internal class Barbers
    {

        private readonly List<Barber> _barbers;

        private readonly Object _syncRoot = new Object();

        public Barbers(Int32 count, AutoResetEvent benchProcessorEvent)
        {
            if (count <= 0)
                throw new ArgumentException(nameof(count));

            _barbers = new List<Barber>(count);
            for(int i = 0; i < count; i++)
            {
                Barber b = new Barber(i, benchProcessorEvent);
                _barbers.Add(b);
            }
        }

        public void Start()
        {
            foreach (var barber in _barbers)
                barber.Start();
        }

        public Barber GetBarberIfReadyForWork()
        {
            lock (_syncRoot)
            {
                return _barbers.FirstOrDefault(x => x.IsBarberFree);
            }
        }
    }
}
