using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class GZipCompress
    {
        //начало выполнения
        DateTime time_start;
        //коллекция потоков
        Thread[] threads_collection;

        //массивы исходных и конечных данных
        byte[][] source_data;
        byte[][] target_data;

        int part_size = 1048576; //1 MB
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

        public GZipCompress(string set_source_file, string set_target_file)
        {
            source_file = set_source_file;
            target_file = set_target_file;

            MyLogWriter = new LogWriter(Path.ChangeExtension(source_file, "log"));

            source_data = new byte[threads_count][];
            target_data = new byte[threads_count][];
        }

        public void CompressFile()
        {
            time_start = DateTime.Now;

            Console.WriteLine($"Cжатие файла: {source_file}\n");

            MyLogWriter.WriteLog($"Cжатие файла: {source_file}");

            using (FileStream source_stream = new FileStream(source_file, FileMode.Open))
            {
                //целевой файл
                using (FileStream target_stream = File.Create(target_file))
                {
                    try
                    {
                        //Создаем коллекцию потоков равную числу процессоров
                        threads_collection = new Thread[threads_count];

                        while (source_stream.Position < source_stream.Length)
                        {
                            //распределяем данные по потокам и сжимаем их
                            CompressParts(source_stream);
                            //объединяем сжатые данные и помещаем их в целевой поток
                            MergeTargetData(target_stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        MyConsole.PrintError(ex.Message);
                        MyLogWriter.WriteLog(ex.Message);
                    }

                    string message = $"\nСжатие файла завершено\nИсходный размер: {source_stream.Length.ToString()} " +
                        $"байт \nРазмер файла после сжатия: {target_stream.Length.ToString()} байт.\nПрошло времен: {DateTime.Now - time_start}";

                    MyConsole.PrintText(message, ConsoleColor.Gray);
                    MyLogWriter.WriteLog(message);
                }
            }
        }
        /// <summary>
        /// Распределить исходные данные по потокам и сжать эти данные
        /// </summary>
        /// <param name="source_stream"></param>
        void CompressParts(FileStream source_stream)
        {
            try
            {
                for (int num = 0; num < threads_count; num++)
                {
                    if (source_stream.Position < source_stream.Length)
                    {
                        //определяем размер не обработанных данных, 
                        //если размер блока сжатия больше, то устанавливаем его в размере остатка
                        if (part_size > source_stream.Length - source_stream.Position)
                        {
                            part_size = (int)(source_stream.Length - source_stream.Position);
                        }

                        //переопределяем переменную на необходимый размер байт
                        source_data[num] = new byte[part_size];
                        // и записываем в нее данные из потока
                        source_stream.Read(source_data[num], 0, part_size);
                        //присваиваем потоку метод обработки данных, который выполнит сжатие
                        threads_collection[num] = new Thread(DataStreamCompression);
                        //и запускаем его, в метод передаем номер потока
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
        /// Сжать поток данных и сохранить в массив сжатых данных
        /// </summary>
        /// <param name="obj"></param>
        void DataStreamCompression(object obj)
        {
            int num = (int)obj;
            try
            {
                using (MemoryStream mem_stream = new MemoryStream(source_data[num].Length))
                {
                    using (GZipStream zip_stream = new GZipStream(mem_stream, CompressionMode.Compress))
                    {
                        zip_stream.Write(source_data[num], 0, source_data[num].Length);
                    }
                    target_data[num] = mem_stream.ToArray();
                    BitConverter.GetBytes(target_data[num].Length).CopyTo(target_data[num], offset);
                }
            }
            catch (Exception ex)
            {
                MyConsole.PrintError(ex.Message);
                MyLogWriter.WriteLog(ex.Message);
            }
        }

        /// <summary>
        /// Объединяем сжатые данные и помещаем их в целевой поток
        /// </summary>
        /// <param name="target_stream"></param>
        void MergeTargetData(FileStream target_stream)
        {
            try
            {
                for (int num = 0; num < threads_count; num++)
                {
                    //Не все блоки могут быть задействованы когда мы подходим к концу файла или когда файл маленький
                    if (threads_collection[num] != null)
                    {
                        //необходимо дождаться завершения потока
                        threads_collection[num].Join();
                        //Выводим номер потока в котором было выполнено сжатие
                        MyConsole.PrintProc(threads_collection[num].ManagedThreadId);
                        //записываем данные из блока в целевой поток
                        target_stream.Write(target_data[num], 0, target_data[num].Length);
                        //обнуляем использованный поток
                        threads_collection[num] = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MyConsole.PrintError(ex.Message);
                MyLogWriter.WriteLog(ex.Message);
            }
        }
    }
}