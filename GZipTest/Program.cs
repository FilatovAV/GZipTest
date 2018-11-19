using System;
using System.IO;

namespace GZipTest
{
    class Program
    {
        /// <summary>
        /// Вспомогательный класс работы с консолью
        /// </summary>
        static ConsoleWrapper MyConsole = new ConsoleWrapper();

        /// <summary>
        /// Запись логов
        /// </summary>
        static LogWriter MyLogWriter;

        //Параметры программы, имена исходного и результирующего файлов должны 
        //задаваться в командной строке следующим образом:
        //GZipTest.exe compress/decompress[имя исходного файла][имя результирующего файла]

        static void Main(string[] args)
        {
            args = new string[3];
            args[0] = "decompress";
            args[1] = "C:\\Test\\big book.gz";
            args[2] = "C:\\Test\\big book.pdf";
            //args = new string[3];
            //args[0] = "compress";
            //args[1] = "C:\\Test\\big book.pdf";
            //args[2] = "C:\\Test\\big book.pdf";

            if (args.Length == 0)
            {
                MyConsole.PrintText($"Для применения программы используйте параметры:\nGZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла]", ConsoleColor.Gray);
                Console.ReadKey();
                return;
            }

            string source_file;
            string target_file;
            string app_command;

            try
            {
                app_command = args[0];
                source_file = args[1];

                MyLogWriter = new LogWriter(Path.ChangeExtension(source_file, "log"));

                //Первичная проверка данных
                if (!FirstCheck(source_file))
                {
                    Console.ReadKey();
                    return;
                }

                switch (app_command.ToLower())
                {
                    case "compress":
                        //Установим расширение целевого файла как .gz в том случае если этого не сделали
                        target_file = args[2].EndsWith(".gz") ? args[2] : args[2] + ".gz";

                        GZipCompress zipCompress = new GZipCompress(source_file, target_file);
                        zipCompress.CompressFile();
                        break;
                    case "decompress":
                        target_file = args[2];

                        GZipDeComppress zipDecomppress = new GZipDeComppress(source_file, target_file);
                        zipDecomppress.DeComppressFile();
                        break;
                    default:
                        MyConsole.PrintText($"Введены недопустимые параметры! {app_command}", ConsoleColor.Red);
                        MyLogWriter.WriteLog($"Введены недопустимые параметры! {app_command}");
                        break;
                }
            }
            catch (Exception ex)
            {
                MyConsole.PrintText($"Проверьте правильность введенных данных.\n{ex.Message}", ConsoleColor.Red);
                MyLogWriter.WriteLog($"Проверьте правильность введенных данных.\n{ex.Message}");
            }

            Console.ReadKey();
        }

        static bool FirstCheck(string source_file)
        {
            if (!File.Exists(source_file))
            {
                MyConsole.PrintText($"Файл с именем \"{source_file}\" не найден!", ConsoleColor.Red);
                MyLogWriter.WriteLog($"Файл с именем \"{source_file}\" не найден!");
                return false;
            }

            FileInfo file = new FileInfo(source_file);
            if (file.Length == 0)
            {
                MyConsole.PrintText($"Файл {source_file} не содержит данных!", ConsoleColor.Red);
                MyLogWriter.WriteLog($"Файл {source_file} не содержит данных!");
                return false;
            }
            return true;
        }
    }
}
