using System;
using System.IO;

namespace GZipTest
{
    class Program
    {
        static MyConsole MyConsole = new MyConsole();

        //Параметры программы, имена исходного и результирующего файлов должны 
        //задаваться в командной строке следующим образом:
        //GZipTest.exe compress/decompress[имя исходного файла][имя результирующего файла]

        static void Main(string[] args)
        {
            //args = new string[3];
            //args[0] = "decompress";
            //args[1] = "C:\\Test\\big book.gz";
            //args[2] = "C:\\Test\\big book.pdf";
            //args = new string[3];
            //args[0] = "compress";
            //args[1] = "C:\\Test\\1";
            //args[2] = "C:\\Test\\1.pdf";

            if (args.Length == 0)
            {
                MyConsole.PrintText($"Для применения программы используйте параметры:\nGZipTest.exe compress/decompress [имя исходного файла] [имя результирующего файла]", ConsoleColor.Gray);
                Console.ReadKey();
                return;
            }

            string sourceFile;
            string targetFile;
            string command;

            try
            {
                command = args[0];
                sourceFile = args[1];

                //Первичная проверка данных
                if (!FirstCheck(sourceFile))
                {
                    Console.ReadKey();
                    return;
                }

                switch (command.ToLower())
                {
                    case "compress":
                        //Установим расширение целевого файла как .gz в том случае если этого не сделали
                        targetFile = args[2].EndsWith(".gz") ? args[2] : args[2] + ".gz";

                        GZipCompress zipCompress = new GZipCompress(sourceFile, targetFile);
                        zipCompress.CompressFile();
                        break;
                    case "decompress":
                        targetFile = args[2];

                        GZipDeComppress zipDeComppress = new GZipDeComppress(sourceFile, targetFile);
                        zipDeComppress.DeComppressFile();
                        break;
                    default:
                        MyConsole.PrintText($"Введены недопустимые параметры! {command}", ConsoleColor.Red);
                        break;
                }
            }
            catch (Exception ex)
            {
                MyConsole.PrintText($"Проверьте правильность введенных данных.\n{ex.Message}", ConsoleColor.Red);
            }

            Console.ReadKey();
        }

        static bool FirstCheck(string sourceFile)
        {
            if (!File.Exists(sourceFile))
            {
                MyConsole.PrintText($"Файл с именем \"{sourceFile}\" не найден!", ConsoleColor.Red);
                return false;
            }

            FileInfo file = new FileInfo(sourceFile);
            long size = file.Length;
            if (size == 0)
            {
                MyConsole.PrintText($"Файл не содержит данных!", ConsoleColor.Red);
                return false;
            }

            return true;
        }
    }
}
