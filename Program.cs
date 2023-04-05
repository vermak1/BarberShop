using System;
using System.Threading;


namespace Hairdresser
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                using(BarberShop b = new BarberShop(10, 5))
                {
                    Thread[] threads = new Thread[200];
                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i] = new Thread((customer) =>
                        {
                            try
                            {
                                if (customer is Customer cust)
                                    b.HandleNewCustomer(cust);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Exception within Thread: {0}\nError: {1}", Thread.CurrentThread.ManagedThreadId, ex.Message);
                            }
                        });
                    }

                    for (int i = 0; i < threads.Length; i++)
                    {
                        threads[i].Start(new Customer() { Name = i.ToString() });
                        Thread.Sleep(100);
                    }
                    Thread.Sleep(Int32.MaxValue);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
                Console.WriteLine("Stack trace: {0}", ex.StackTrace);
                Environment.Exit(1);
            }
        }
    }
}
