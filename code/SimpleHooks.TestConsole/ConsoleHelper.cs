using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTools.SimpleHooks.TestConsole
{
    internal class ConsoleHelper
    {
        internal static void WriteConsoleStart(string methodName)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{methodName} started");
        }

        internal static void WriteConsoleCompleted(string methodName, string result)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"{methodName} completed - result: {result}");
            Console.WriteLine("-------");
            Console.WriteLine();
        }

        internal static void WriteConsoleError(string methodName, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"{methodName} threw error: {e}");
            Console.WriteLine("-------");
            Console.WriteLine();
        }
    }
}
