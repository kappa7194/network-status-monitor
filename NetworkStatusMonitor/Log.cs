namespace NetworkStatusMonitor
{
    using System.Globalization;

    using NLog;

    public class Log
    {
        private readonly ILogger logger;

        private readonly static Log ApplicationInstance = new Log("application");
        private readonly static Log ExceptionsInstance = new Log("exceptions");
        private readonly static Log StatusInstance = new Log("status");

        private Log(string loggerName)
        {
            this.logger = LogManager.GetLogger(loggerName);
        }

        public static Log Application => ApplicationInstance;

        public static Log Exceptions => ExceptionsInstance;

        public static Log Status => StatusInstance;

        public void Debug(string format, params object[] args)
        {
            this.Write(LogLevel.Debug, format, args);
        }

        public void Debug(string message)
        {
            this.Write(LogLevel.Debug, message);
        }

        public void Trace(string format, params object[] args)
        {
            this.Write(LogLevel.Trace, format, args);
        }

        public void Trace(string message)
        {
            this.Write(LogLevel.Trace, message);
        }

        public void Info(string format, params object[] args)
        {
            this.Write(LogLevel.Info, format, args);
        }

        public void Info(string message)
        {
            this.Write(LogLevel.Info, message);
        }

        public void Warn(string format, params object[] args)
        {
            this.Write(LogLevel.Warn, format, args);
        }

        public void Warn(string message)
        {
            this.Write(LogLevel.Warn, message);
        }

        public void Error(string format, params object[] args)
        {
            this.Write(LogLevel.Error, format, args);
        }

        public void Error(string message)
        {
            this.Write(LogLevel.Error, message);
        }

        public void Fatal(string format, params object[] args)
        {
            this.Write(LogLevel.Fatal, format, args);
        }

        public void Fatal(string message)
        {
            this.Write(LogLevel.Fatal, message);
        }

        private void Write(LogLevel level, string format, params object[] args)
        {
            var message = string.Format(CultureInfo.InvariantCulture, format, args);

            this.Write(level, message);
        }

        private void Write(LogLevel level, string message)
        {
            this.logger.Log(level, message);
        }
    }
}
