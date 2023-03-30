using Hairdresser;
using System.Runtime.ExceptionServices;

namespace BarberShopTests
{
    public class BarberShop_Threading_Tests
    {
        [Test]
        [Repeat(50)]
        public void SingleCustomer_Test([Values(1, 5)] int x)
        {
            BarberShop b = new BarberShop(x);
            b.StartWork();
            Customer c = new Customer();

            bool success = b.ProcessNewCustomer(c);
            Assert.IsTrue(success);
        }

        [Test]
        [Repeat(10)]
        public void MultipleCustomersSimultaneously_Test([Values(100, 500, 5000)] int threadNumber, [Values(1, 5, 100)] int capacity)
        {
            BarberShop b = new BarberShop(capacity);
            b.StartWork();

            Thread[] ts = new Thread[threadNumber];
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i] = new Thread((customer) =>
                {
                    try
                    {
                        if (customer is Customer cust)
                            b.ProcessNewCustomer(cust);
                    }
                    catch
                    { 
                        Console.WriteLine("Exception within Thread: {0}", Thread.CurrentThread.ManagedThreadId);
                    }
                });
            }

            for (int i = 0; i < ts.Length; i++)
                ts[i].Start(new Customer() { Name = i.ToString() });


            foreach (Thread t in ts)
                t.Join();

            Assert.That(b.MaxCapacityOccured <= b.Capacity);
        }

        [Test]
        public void MultipleCustomersWithDelays_Test([Values(100, 500, 5000)] int threadNumber, [Values(1, 15)] int capacity, [Values(100, 500)] int timeout)
        {
            BarberShop b = new BarberShop(capacity);
            b.StartWork();

            Thread[] ts = new Thread[threadNumber];
            for (int i = 0; i < ts.Length; i++)
            {
                ts[i] = new Thread((customer) =>
                {
                    try
                    {
                        if (customer is Customer cust)
                            b.ProcessNewCustomer(cust);
                    }
                    catch
                    { 
                        Console.WriteLine("Exception within Thread: {0}", Thread.CurrentThread.ManagedThreadId); 
                    }
                });
            }

            for (int i = 0; i < ts.Length; i++)
            {
                ts[i].Start(new Customer() { Name = i.ToString() });
                if (i % 50 == 0)
                    Thread.Sleep(timeout);
            }

            Assert.That(b.MaxCapacityOccured <= b.Capacity);
        }
    }


    public class BarberShop_Negative_Tests
    {
        [Test]
        public void NullCustomer_NegativeTest()
        {
            BarberShop b = new BarberShop(5);
            b.StartWork();

            Customer cust = null;

            Assert.Throws<ArgumentNullException>(() =>
            {
                b.ProcessNewCustomer(cust);
            });
        }

        [Test]
        public void NegativeOrZeroCapacity_NegativeTest([Values(-2, 0)] int x)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                BarberShop b = new BarberShop(x);
            });
        }
    }
}