using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace FewsCommon
{
    /// <summary>
    /// Used for logging to FEWS log file
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Log level
        /// </summary>
        public enum TypeEnum : int
        {
            Info,
            Warning,
            Error
        }

        private static  string _path;
        private static bool _buffered;
        private static StringBuilder _sb;

        /// <summary>
        /// Initialize logger
        /// </summary>
        /// <param name="path">Log file Name</param>
        /// <param name="buffered">If true, buffered to memory. If false - write
        /// immediately to file (slower)</param>
        public static void Initialize(string path, bool buffered)
        {
            _path = path;
            _buffered = buffered;
            if (_buffered)
            {
                _sb = new StringBuilder();
            }

            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Add one log message
        /// </summary>
        /// <param name="type">Log Level</param>
        /// <param name="text">Log message text</param>
        public static void AddLog(TypeEnum type, string text)
        {
            if (_buffered)
            {
                _sb.Append(DateTime.Now);
                _sb.Append(" ");
                _sb.Append(type.ToString().ToUpper());
                _sb.Append(" ");
                _sb.AppendLine(text);
            }
            else
            {
                File.AppendAllText(_path, DateTime.Now + " " + type.ToString().ToUpper() + " " + text + Environment.NewLine);
            }
        }

        /// <summary>
        /// Write logs from memory to file.
        /// </summary>
        public static void Write()
        {
            if (_sb != null)
            {
                File.AppendAllText(_path, _sb.ToString());
                _sb.Clear();
            }
        }
    }
}
