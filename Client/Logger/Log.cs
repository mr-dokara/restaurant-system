using System;
using System.Text.RegularExpressions;

namespace Logger
{
    /// <summary>
    /// Статический класс для логирования.
    /// </summary>
    public static class Log
    {
        private static string _fileName;

        /// <summary>
        /// Возвращает и задает имя файла.
        /// <para><b>Расширение задавать не нужно.</b></para>
        /// </summary>
        public static string FileName
        {
            get => _fileName;
            set
            {
                var fileNameChecker = new Regex(@"^([A-Za-z0-9_\.]+)$");

                if (fileNameChecker.IsMatch(value))
                    _fileName = value + ".log";
                else
                    throw new ArgumentException();
            }
        }
    }
}