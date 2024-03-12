using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoKatan.Static
{
    public static class StringExtensions
    {
        public static void Print(this string st)
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(st);
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        public static void PrintAlert(this string st)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(st);
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        public static void PrintCancelled(this string st)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(st);
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }

        public static void PrintDisposed(this string st)
        {
            Console.ForegroundColor = ConsoleColor.DarkMagenta;
            Console.WriteLine(st);
            Console.ForegroundColor = ConsoleColor.DarkGray;
        }
    }
}
