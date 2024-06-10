using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        public static string GetEnumDescription(this Enum value)
        {
            DescriptionAttribute[] customAttributes = (DescriptionAttribute[])value.GetType().GetField(value.ToString()).GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customAttributes != null && customAttributes.Length != 0 ? customAttributes[0].Description : value.ToString();
        }

        public static TEnum GetEnumValue<TEnum>(this string description) where TEnum : Enum
        {
            if (string.IsNullOrEmpty(description))
            {
                return default;
            }

            var map = Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .ToDictionary(e => e.GetEnumDescription());

            if (map.TryGetValue(description, out TEnum result))
            {
                return result;
            }

            return default;
        }
    }
}
