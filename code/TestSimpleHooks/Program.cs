using System;

namespace TestSimpleHooks
{
    class Program
    {
        static void Main()
        {
            Start();
            Console.WriteLine("Press any key to exit...");
            Console.Read();
        }

        static void Start()
        {
            //TestSimpleHttpClient.Test1();
            TestActualOperation.TestAddEventInstance1();
            //TestActualOperation.TestProcess1();
            TestActualOperation.TestAddEventInstance2();
            TestActualOperation.TestProcess2();
        }
    }
}
