using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GZipTest
{
    public class LogWriter
    {
        string log_file_name;

        public LogWriter(string file_name)
        {
            log_file_name = file_name;
        }
        /// <summary>
        /// Выполняет запись сообщений в лог файл.
        /// </summary>
        /// <param name="log_message">Сообщение для записи в лог файл.</param>
        public void WriteLog(string log_message)
        {
            using (StreamWriter sw = new StreamWriter(log_file_name, true))
            {
                sw.WriteLine($"{DateTime.Now}: {log_message.Replace("\n", Environment.NewLine)}");
            }
        }
    }
}
