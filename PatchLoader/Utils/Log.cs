using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PatchLoader.Utils {
   public class Log {
        private enum LogLevel {
            Debug,
            Info,
            Error,
            Exception
        }

        private static readonly object LogLock = new object();
        private static readonly string LogFilename = Path.Combine(Paths.WorkingPath, "PatchLoader.log");
        private static Stopwatch sw = Stopwatch.StartNew();
        private readonly string _prefix;

        static Log() {
            try {
                if (File.Exists(LogFilename)) {
                    File.Delete(LogFilename);
                }
            } catch (Exception) {
            }
        }

        [Conditional("DEBUG")]
        public static void _Debug(string s, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null) {
            LogToFileExtended(s, LogLevel.Debug, memberName, filePath);
        }

        public static void Info(string s) {
            LogToFile(s, LogLevel.Info);
        }

        public static void Error(string s) {
            LogToFile(s, LogLevel.Error);
        }
        
        public static void Exception(Exception e, string s = "") {
            LogToFile(s.Length > 0
                    ? s + "\n" + e.Message + "\n" + e.StackTrace 
                    : e.Message + "\n" + e.StackTrace, 
                LogLevel.Exception);
        }

        private static void LogToFile(string log, LogLevel level) {
            try {
                Monitor.Enter(LogLock);

                long secs = sw.ElapsedTicks / Stopwatch.Frequency;
                long fraction = sw.ElapsedTicks % Stopwatch.Frequency;
                using (StreamWriter w = File.AppendText(LogFilename)) {
                    w.Write("[{0, -9}]@ {1, 4}:{2:D7} | ", level.ToString(), secs, fraction);
                    w.WriteLine(log);
                    if (level == LogLevel.Error) {
                        w.WriteLine(new StackTrace().ToString());
                        w.WriteLine();
                    }
                }
            } finally {
                Monitor.Exit(LogLock);
            }
        }

        /// <summary>
        /// Logs message to file with additional info about caller (file + method name)
        /// </summary>
        /// <param name="log"></param>
        /// <param name="level"></param>
        /// <param name="memberName"></param>
        /// <param name="filePath"></param>
        private static void LogToFileExtended(string log, LogLevel level, string memberName, string filePath) {
            try {
                Monitor.Enter(LogLock);

                long secs = sw.ElapsedTicks / Stopwatch.Frequency;
                long fraction = sw.ElapsedTicks % Stopwatch.Frequency;
                using (StreamWriter w = File.AppendText(LogFilename)) {
                    w.Write("[{0, -9}]@ {1, 4}:{2:D7} [{3}.{4}] ", level.ToString(), secs, fraction, Path.GetFileNameWithoutExtension(filePath), memberName);
                    w.WriteLine(log);
                    if (level == LogLevel.Error) {
                        w.WriteLine(new StackTrace().ToString());
                        w.WriteLine();
                    }
                }
            } finally {
                Monitor.Exit(LogLock);
            }
        }
   }
}