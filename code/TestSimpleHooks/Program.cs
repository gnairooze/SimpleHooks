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
            TestActualOperation.TestAddEventInstance1();
        }
    }
}
