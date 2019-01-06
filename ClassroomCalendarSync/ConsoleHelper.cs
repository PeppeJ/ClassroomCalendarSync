using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClassroomCalendarSync
{
    public static class ConsoleHelper
    {
        public static ConsoleColor InfoColor { get; } = ConsoleColor.Cyan;
        public static ConsoleColor ErrorColor { get; } = ConsoleColor.Red;
        public static ConsoleColor SuccessColor { get; } = ConsoleColor.Green;


        public static void Info(string message) => PrintColor(message, InfoColor);
        public static void Error(string message) => PrintColor(message, ErrorColor);
        public static void Success(string message = "") => PrintColor($"Success: {message}", SuccessColor);

        public static void PrintColor(string message, ConsoleColor fore, ConsoleColor back = ConsoleColor.Black)
        {
            Console.ForegroundColor = fore;
            Console.BackgroundColor = back;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
