using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class GZipDeComppress
    {
        //начало выполнения
        DateTime time_start;
        //коллекция потоков
        Thread[] threads_collection;

        //массивы исходных и конечных данных
        byte[][] source_data;
        byte[][] target_data;

        readonly int offset = 4; //когда мы разбиваем на части необходимо сместиться на 4 байта
        readonly int threads_count = Environment.ProcessorCount;

        readonly string source_file;
        readonly string target_file;

        /// <summary>
        /// работа с консолью
        /// </summary>
        ConsoleWrapper MyConsole = new ConsoleWrapper();
        /// <summary>
        /// Запись логов
        /// </summary>
        LogWriter MyLogWriter;

        public GZipDeComppress(string set_source_file, string set_target_file)
        {
            source_file = set_source_file;
            target_file = set_target_file;

            MyLogWriter = new LogWriter(Path.ChangeExtension(source_file, "log"));

            source_data = new byte[threads_count][];
            target_data = new byte[threads_count][];
        }

        /// <summary>
        /// Извлечение данных из сжатого файла
        /// </summary>
        public void DeComppressFile()
        {
            time_start = DateTime.Now;
            try
            {
                using (FileStream source_stream = new FileStream(source_file, FileMode.Open))
                {
                    using (FileStream target_stream = File.Create(target_file))
                    {
                        Console.WriteLine($"Выполняется извлечение из архива {source_file}.\n");
                        MyLogWriter.WriteLog($"Выполняется извлечение из архива {source_file}.");
                        while (source_stream.Position < source_stream.Length)
                        {
                            //Создаем коллекцию потоков
                            threads_collection = new Thread[threads_count];
                            //распределяем данные по потокам и расжимаем их
                            DeComppressParts(source_stream);
                            //объединяем полученные данные и помещаем их в целевой поток
                            MergeTargetData(target_stream);
                        }

                        string message = $"\n\nУспешное завершение\nФайл: {target_file}\nПрошло времени: {DateTime.Now - time_start}";
                        MyConsole.PrintText(message, ConsoleColor.Gray);
                        MyLogWriter.WriteLog(message);
                    }
                }
            }
            catch (Exception ex)
            {
                MyConsole.PrintError(ex.Message);
                MyLogWriter.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// Объединение блоков данных
        /// </summary>
        /// <param name="target_stream"></param>
        private void MergeTargetData(FileStream target_stream)
        {
            try
            {
                for (int num = 0; num < threads_count; num++)
                {
                    if (threads_collection[num] != null)
                    {
                        threads_collection[num].Join();
                        target_stream.Write(source_data[num], 0, source_data[num].Length);
                    }
                }
            }
            catch (Exception ex)
            {
                MyConsole.PrintError(ex.Message);
                MyLogWriter.WriteLog(ex.Message);
            }
        }
        /// <summary>
        /// Извлечение блоков
        /// </summary>
        /// <param name="source_stream"></param>
        private void DeComppressParts(FileStream source_stream)
        {
            //http://www.zlib.org/rfc-gzip.html#member-format
            //  0   1   2   3   4   5   6   7
            //+---+---+---+---+---+---+---+---+
            //|     CRC32     |     ISIZE     |
            //+---+---+---+---+---+---+---+---+

            try
            {
                //записываются в начало каждого блока
                int zip_info_data = 8;
                byte[] buffer = new byte[zip_info_data];
                int part_size;
                //длина блока вместе с заголовком
                int zip_part_size;
                for (int num = 0; num < threads_count; num++)
                {
                    if (source_stream.Position < source_stream.Length)
                    {
                        //получаем длину блока
                        source_stream.Read(buffer, 0, zip_info_data);
                        //определяем размер части
                        zip_part_size = BitConverter.ToInt32(buffer, offset);
                        //создаем пустой блок сжатых данных в памяти
                        target_data[num] = new byte[zip_part_size];
                        //записываем данные в целевой массив
                        buffer.CopyTo(target_data[num], 0);
                        //считываем только данные без заголовка
                        source_stream.Read(target_data[num], zip_info_data, zip_part_size - zip_info_data);
                        //получаем чистый размер блока данных
                        part_size = BitConverter.ToInt32(target_data[num], zip_part_size - offset);

                        source_data[num] = new byte[part_size];
                        threads_collection[num] = new Thread(DataStreamDeComppression);
                        threads_collection[num].Start(num);
                    }
                }
            }
            catch (Exception ex)
            {
                MyConsole.PrintError(ex.Message);
                MyLogWriter.WriteLog(ex.Message);
            }
        }
        /// <summary>
        /// Выполнить декомпрессию данных
        /// </summary>
        /// <param name="obj"></param>
        void DataStreamDeComppression(object obj)
        {
            try
            {
                int num = (int)obj;
                using (MemoryStream ms = new MemoryStream(target_data[num]))
                {
                    using (GZipStream zip_stream = new GZipStream(ms, CompressionMode.Decompress))
                    {
                        zip_stream.Read(source_data[num], 0, source_data[num].Length);
                    }
                }
                //Выводим номер потока в котором было выполнено извлечение данных
                MyConsole.PrintProc(num);
            }
            catch (Exception ex)
            {
                MyConsole.PrintError(ex.Message);
                MyLogWriter.WriteLog(ex.Message);
            }
        }
    }
}
