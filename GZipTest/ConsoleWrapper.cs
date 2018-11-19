using System;
using System.IO;

namespace GZipTest
{
    class ConsoleWrapper
    {
        /// <summary>
        /// Отображение номера процесса в консоли с распределением цвета по четности
        /// </summary>
        /// <param name="num"></param>
        public void PrintProc(int num)
        {
            if (num % 2 > 0)
            {
                PrintText(num.ToString(), ReturnConsoleColor(num));
            }
            else
            {
                PrintTextInLine(num.ToString(), ReturnConsoleColor(num));
            }
        }
        /// <summary>
        ///  Печать текста (и с указанием цвета)
        /// </summary>
        /// <param name="message"></param>
        public void PrintText(string message)
        {
            Console.WriteLine(message);
        }
        public void PrintText(string message, ConsoleColor message_color)
        {
            Console.ForegroundColor = message_color;
            Console.WriteLine(message);
        }
        public void PrintError(string log_message)
        {
            PrintText(log_message, ConsoleColor.Red);
        }
        /// <summary>
        /// Печать текста в линии
        /// </summary>
        /// <param name="message"></param>
        /// <param name="message_color"></param>
        public void PrintTextInLine(string message, ConsoleColor message_color)
        {
            Console.ForegroundColor = message_color;
            Console.Write(message);
        }
        /// <summary>
        /// Вернуть цвет консоли по номеру потока
        /// </summary>
        /// <param name="num"></param>
        public ConsoleColor ReturnConsoleColor(int num)
        {
            return (num + 1) % 2 > 0 ? Console.ForegroundColor = ConsoleColor.Green : Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
    }
}
