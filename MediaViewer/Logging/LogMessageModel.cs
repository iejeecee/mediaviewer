using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Logging
{
    public class LogMessageModel
    {
        public enum LogLevel
        {
            UNKNOWN,
            FATAL,
            ERROR,
            WARNING,
            INFO,
            DEBUG
        }

        public LogMessageModel(LogLevel level, String message)
        {
            Level = level;
            Text = message;
        }

        LogLevel level;

        public LogLevel Level
        {
            get { return level; }
            set { level = value; }
        }

        String text;

        public String Text
        {
            get { return text; }
            set { text = value; }
        }
    }
}
