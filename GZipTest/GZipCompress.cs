using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace GZipTest
{
    class GZipCompress
    {
        byte[][] sourceData;
        byte[][] gzipData;
        readonly string sourceFile;
        readonly string targetFile;
        readonly int threadsCount = Environment.ProcessorCount;
        DateTime tS;
        Thread[] ThreadCollection;
        readonly int offsetMark = 4; //когда мы разбиваем на части необходимо сместиться на 4 байта

        /// <summary>
        /// работа с консолью чуть удобней
        /// </summary>
        MyConsole myConsole = new MyConsole();

        public GZipCompress(string sFile, string tFile)
        {
            sourceFile = sFile;
            targetFile = tFile;
        }

        public void CompressFile()
        {
            int partSize = 1048576; //1 MB
            tS = DateTime.Now;

            sourceData = new byte[threadsCount][];
            gzipData = new byte[threadsCount][];

            try
            {
                Console.WriteLine($"Cжатие файла: {sourceFile}\n");

                using (FileStream sourceStream = new FileStream(sourceFile, FileMode.Open))
                {
                    //целевой файл
                    using (FileStream targetStream = File.Create(targetFile))
                    {
                        while (sourceStream.Position < sourceStream.Length)
                        {
                            //Создаем коллекцию потоков
                            ThreadCollection = new Thread[threadsCount];
                            //распределяем данные по потокам и сжимаем их
                            CompressParts(partSize, sourceStream);
                            //объединяем сжатые данные и помещаем их в целевой поток
                            MergeTargetData(targetStream);
                        }

                        myConsole.PrintText($"\n\nИсходный размер: {sourceStream.Length.ToString()} " +
                            $"байт \nРазмер файла после сжатия: {targetStream.Length.ToString()} байт.\nПрошло времен: {DateTime.Now - tS}", ConsoleColor.Gray);
                    }
                }
            }
            catch (Exception ex)
            {
                myConsole.PrintText(ex.Message, ConsoleColor.Red);
            }
        }

        #region Обслуживающие методы
        /// <summary>
        /// Распределить исходные данные по потокам и сжать эти данные
        /// </summary>
        /// <param name="partSize"></param>
        /// <param name="sourceStream"></param>
        void CompressParts(int partSize, FileStream sourceStream)
        {
            for (int iPart = 0; iPart < threadsCount; iPart++)
            {
                if (sourceStream.Position < sourceStream.Length)
                {
                    //определяем размер данных, если размер по умолчанию больше то устанавливаем его в размере остатка
                    if (partSize > sourceStream.Length - sourceStream.Position)
                    {
                        partSize = (int)(sourceStream.Length - sourceStream.Position);
                    }

                    //переопределяем переменную и записываем из нее данные из потока
                    sourceData[iPart] = new byte[partSize];
                    sourceStream.Read(sourceData[iPart], 0, partSize);
                    //присваиваем потоку метод обработки данных и запускаем его, в метод передаем номер потока 
                    ThreadCollection[iPart] = new Thread(DataStreamCompression);
                    ThreadCollection[iPart].Start(iPart);
                }
            }
        }

        /// <summary>
        /// Сжать поток данных и сохранить в массив сжатых данных
        /// </summary>
        /// <param name="obj"></param>
        void DataStreamCompression(object obj)
        {
            int thread = (int)obj;
            using (MemoryStream ms = new MemoryStream(sourceData[thread].Length))
            {
                using (GZipStream zips = new GZipStream(ms, CompressionMode.Compress))
                {
                    zips.Write(sourceData[thread], 0, sourceData[thread].Length);
                }
                gzipData[thread] = ms.ToArray();
                BitConverter.GetBytes(gzipData[thread].Length).CopyTo(gzipData[thread], offsetMark);
            }

            //Выводим номер потока в котором было выполнено сжатие
            myConsole.PrintProc(thread);
        }

        /// <summary>
        /// Объединяем сжатые данные и помещаем их в целевой поток
        /// </summary>
        /// <param name="targetStream"></param>
        void MergeTargetData(FileStream targetStream)
        {
            for (int iPart = 0; iPart < threadsCount; iPart++)
            {
                //Не все блоки могут быть задействованы когда мы подходим к концу файла или когда файл маленький
                if (ThreadCollection[iPart] != null)
                {
                    //необходимо дождаться завершения потока иначе может быть null
                    ThreadCollection[iPart].Join();
                    //записываем данные из блока в целевой поток
                    targetStream.Write(gzipData[iPart], 0, gzipData[iPart].Length);
                }
            }
        }
        #endregion
    }
}
