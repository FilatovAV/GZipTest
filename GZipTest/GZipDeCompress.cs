using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class GZipDeComppress
    {
        byte[][] sourceData;
        byte[][] gzipData;
        readonly string sourceFile;
        readonly string targetFile;
        readonly int threadsCount = Environment.ProcessorCount;
        DateTime tS;
        Thread[] ThreadCollection;
        readonly int offsetMark = 4;
        readonly ConsoleColor defCc = ConsoleColor.Gray;

        /// <summary>
        /// работа с консолью чуть удобней
        /// </summary>
        MyConsole myConsole = new MyConsole();
        public GZipDeComppress(string sFile, string tFile)
        {
            sourceFile = sFile;
            targetFile = tFile;
        }

        /// <summary>
        /// Извлечение данных из сжатого файла
        /// </summary>
        public void DeComppressFile()
        {
            tS = DateTime.Now;
            sourceData = new byte[threadsCount][];
            gzipData = new byte[threadsCount][];

            try
            {
                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open))
                {
                    using (FileStream targetStream = File.Create(targetFile))
                    {
                        Console.WriteLine($"Выполняется извлечение из архива {sourceFile}.\n");
                        while (sourceStream.Position < sourceStream.Length)
                        {
                            //Создаем коллекцию потоков
                            ThreadCollection = new Thread[threadsCount];
                            //распределяем данные по потокам и расжимаем их
                            DeComppressParts(sourceStream);
                            //объединяем полученные данные и помещаем их в целевой поток
                            MergeTargetData(targetStream);
                        }

                        myConsole.PrintText($"\n\nУспешное завершение\nФайл: {targetFile}\nПрошло времени: {DateTime.Now - tS}", defCc);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadKey();
            }
        }

        #region Обслуживающие методы
        /// <summary>
        /// Объединение блоков данных
        /// </summary>
        /// <param name="targetStream"></param>
        private void MergeTargetData(FileStream targetStream)
        {
            for (int partCounter = 0; partCounter < threadsCount; partCounter++)
            {
                if (ThreadCollection[partCounter] != null)
                {
                    ThreadCollection[partCounter].Join();
                    targetStream.Write(sourceData[partCounter], 0, sourceData[partCounter].Length);
                }
            }
        }
        /// <summary>
        /// Извлечение блоков
        /// </summary>
        /// <param name="sourceStream"></param>
        private void DeComppressParts(FileStream sourceStream)
        {
            int dataLength = 8;
            byte[] buffer = new byte[dataLength];
            int partSize;
            int zipPartSize;
            for (int iPart = 0; iPart < threadsCount; iPart++)
            {
                if (sourceStream.Position < sourceStream.Length)
                {
                    sourceStream.Read(buffer, 0, dataLength);
                    //Определяем размер блока
                    zipPartSize = BitConverter.ToInt32(buffer, offsetMark);
                    gzipData[iPart] = new byte[zipPartSize];
                    buffer.CopyTo(gzipData[iPart], 0);
                    sourceStream.Read(gzipData[iPart], dataLength, zipPartSize - dataLength);
                    partSize = BitConverter.ToInt32(gzipData[iPart], zipPartSize - offsetMark);
                    sourceData[iPart] = new byte[partSize];
                    ThreadCollection[iPart] = new Thread(DataStreamDeComppression);
                    ThreadCollection[iPart].Start(iPart);
                }
            }
        }
        /// <summary>
        /// Выполнить декомпрессию данных
        /// </summary>
        /// <param name="obj"></param>
        void DataStreamDeComppression(object obj)
        {
            int thread = (int)obj;
            using (MemoryStream ms = new MemoryStream(gzipData[thread]))
            {
                using (GZipStream uzips = new GZipStream(ms, CompressionMode.Decompress))
                {
                    uzips.Read(sourceData[thread], 0, sourceData[thread].Length);
                }
            }
            //Выводим номер потока в котором было выполнено извлечение данных
            myConsole.PrintProc(thread);
        }
        #endregion

    }
}
