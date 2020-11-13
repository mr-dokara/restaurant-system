using System;
using System.IO;
using System.Text;
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

        /// <summary>
        /// <para>Формат строки лога.</para>
        /// <para><b>%D</b> - формат даты DD.MM.YYYY</para>
        /// <para><b>%dd</b> или <b>%d</b> - день</para>
        /// <para><b>%mm</b> или <b>%m</b> - месяц</para>
        /// <para><b>%yyyy</b> или <b>%yy</b> - год</para>
        /// <para><b>%mes</b> - сообщение</para>
        /// <para><b>%T</b> - формат времени HH:MM:SS</para>
        /// <para><b>%HH</b> - часы</para>
        /// <para><b>%MM</b> - минуты</para>
        /// <para><b>%SS</b> - секунды</para>
        /// </summary>
        public static string CustomFormat { get; set; }

        static Log()
        {
            FileName = "logs";
            CustomFormat = "[%D %T] %mes";
        }

        private static string GetCorrectLogString(string message, DateTime time)
        {
            var corrector = new Regex(@"(%(D|T|mes|dd?|mm?|yy(yy)?|HH?|MM?|SS?)|\S|\s)");
            var sb = new StringBuilder();

            foreach (Match match in corrector.Matches(CustomFormat))
            {
                var stringMatch = match.ToString();

                switch (stringMatch)
                {
                    case "%D":
                        sb.Append($"{time.Day:D2}.{time.Month:D2}.{time.Year:D2}");
                        break;
                    case "%T":
                        sb.Append($"{time.Hour:D2}:{time.Minute:D2}:{time.Second:D2}");
                        break;
                    case "%mes":
                        sb.Append(message);
                        break;
                    case "%d":
                    case "%dd":
                        sb.Append(stringMatch == "%d" ? $"{time.Day:D1}" : $"{time.Day:D2}");
                        break;
                    case "%m":
                    case "%mm":
                        sb.Append(stringMatch == "%m" ? $"{time.Month:D1}" : $"{time.Month:D2}");
                        break;
                    case "%yy":
                    case "%yyyy":
                        sb.Append(stringMatch == "%yy" ? $"{time.Year:D2}" : $"{time.Year:D4}");
                        break;
                    case "%H":
                    case "%HH":
                        sb.Append(stringMatch == "%H" ? $"{time.Hour:D1}" : $"{time.Hour:D2}");
                        break;
                    case "%M":
                    case "%MM":
                        sb.Append(stringMatch == "%M" ? $"{time.Hour:D1}" : $"{time.Hour:D2}");
                        break;
                    case "%S":
                    case "%SS":
                        sb.Append(stringMatch == "%S" ? $"{time.Second:D1}" : $"{time.Second:D2}");
                        break;
                    default:
                        sb.Append(stringMatch);
                        break;
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Добавляет запись в файл лога.
        /// </summary>
        /// <param name="message">Сообщение, которое будет добавлено.</param>
        public static void AddNote(string message)
        {
            using (var stream = new FileStream(FileName, FileMode.Append))
            {
                var time = DateTime.Now;
                var bytes = Encoding.UTF8.GetBytes(GetCorrectLogString(message, time) + "\n");
                stream.Write(bytes, 0, bytes.Length);
            }
        }
    }
}