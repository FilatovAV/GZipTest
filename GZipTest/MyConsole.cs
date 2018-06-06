using System;

namespace GZipTest
{
    class MyConsole
    {
        /// <summary>
        /// Отображение номера процесса в консоли с распределением цвета по четности
        /// </summary>
        /// <param name="iThread"></param>
        public void PrintProc(int iThread)
        {
            if (iThread % 2 > 0)
            {
                PrintText(iThread.ToString(), ReturnConsoleColor(iThread));
            }
            else
            {
                PrintTextInLine(iThread.ToString(), ReturnConsoleColor(iThread));
            }
        }
        /// <summary>
        /// Печать текста (и с указанием цвета)
        /// </summary>
        /// <param name="text"></param>
        public void PrintText(string text)
        {
            Console.WriteLine(text);
        }
        public void PrintText(string text, ConsoleColor textColor)
        {
            Console.ForegroundColor = textColor;
            Console.WriteLine(text);
        }
        /// <summary>
        /// Печать текста в линии
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textColor"></param>
        public void PrintTextInLine(string text, ConsoleColor textColor)
        {
            Console.ForegroundColor = textColor;
            Console.Write(text);
        }
        /// <summary>
        /// Вернуть цвет консоли по номеру потока
        /// </summary>
        /// <param name="iThread"></param>
        public ConsoleColor ReturnConsoleColor(int iThread)
        {
            return (iThread + 1) % 2 > 0 ? Console.ForegroundColor = ConsoleColor.Green : Console.ForegroundColor = ConsoleColor.DarkGreen;
        }
    }
}
