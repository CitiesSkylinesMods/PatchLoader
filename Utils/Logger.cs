using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Utils
{
    public class Logger
    {
        private readonly object _lock = new object();
        private readonly Stopwatch _sw = Stopwatch.StartNew();
        private readonly string _logFilePath;

        public Logger(string logFilePath)
        {
            _logFilePath = logFilePath ?? throw new ArgumentNullException(nameof(logFilePath));
        }

        [Conditional("DEBUG")]
        public void _Debug(string s, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null)
        {
            LogToFileExtended(s, LogLevel.Debug, memberName, filePath);
        }

        public void Info(string s)
        {
            LogToFile(s, LogLevel.Info);
        }

        public void Error(string s)
        {
            LogToFile(s, LogLevel.Error);
        }

        public void Exception(Exception e, string s = "")
        {
            LogToFile(s.Length > 0
                    ? s + "\n" + e.Message + "\n" + e.StackTrace
                    : e.Message + "\n" + e.StackTrace,
                LogLevel.Exception);
        }

        private void LogToFile(string log, LogLevel level)
        {
            lock (_lock)
            {
                long secs = _sw.ElapsedTicks / Stopwatch.Frequency;
                long fraction = _sw.ElapsedTicks % Stopwatch.Frequency;
                using (StreamWriter w = File.AppendText(_logFilePath))
                {
                    w.Write("[{0, -9}]@ {1, 4}:{2:D7} | ", level.ToString(), secs, fraction);
                    w.WriteLine(log);
                    if (level == LogLevel.Error)
                    {
                        w.WriteLine(new StackTrace().ToString());
                        w.WriteLine();
                    }
                }
            }
        }

        /// <summary>
        /// Logs message to file with additional info about caller (file + method name)
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        private void LogToFileExtended(string log, LogLevel level, string memberName, string filePath)
        {
            lock (_lock)
            {
                long secs = _sw.ElapsedTicks / Stopwatch.Frequency;
                long fraction = _sw.ElapsedTicks % Stopwatch.Frequency;
                using (StreamWriter w = File.AppendText(_logFilePath))
                {
                    w.Write("[{0, -9}]@ {1, 4}:{2:D7} [{3}.{4}] ", level.ToString(), secs, fraction, Path.GetFileNameWithoutExtension(filePath), memberName);
                    w.WriteLine(log);
                    if (level == LogLevel.Error)
                    {
                        w.WriteLine(new StackTrace().ToString());
                        w.WriteLine();
                    }
                }
            }
        }

        private enum LogLevel
        {
            Debug,
            Info,
            Error,
            Exception
        }
    }
}
