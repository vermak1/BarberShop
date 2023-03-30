using System;
using System.Collections.Generic;
using System.Threading;

namespace Hairdresser
{
    internal class Barbers
    {
        private readonly List<Barber> _barbers;

        private readonly Object _syncRoot = new Object();

        public Barbers(Int32 count, AutoResetEvent waitingManagerEvent)
        {
            _barbers = new List<Barber>(count);
            for(int i = 0; i < count; i++)
            {
                Barber b = new Barber(i, waitingManagerEvent);
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
                for (int i = 0; i < _barbers.Count; i++)
                {
                    if (_barbers[i].BarberChair.Customer == null)
                        return _barbers[i];
                }
                return null;
            }
        }
    }
}
