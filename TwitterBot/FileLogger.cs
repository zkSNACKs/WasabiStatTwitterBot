using System;
using System.IO;
using System.Text;

namespace TwitterBot 
{
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Information = 2,
        Warning = 3,
        Error = 4,
        Critical = 5,
        None = 6
    }

	public class FileLogger
	{
		private readonly string _filePath;
		private readonly static object _sync = new object();
		public LogLevel MinLevel { get; set; }

		public FileLogger(string filePath) {
			this._filePath = filePath;
			this.MinLevel = LogLevel.Information;			
		}

		public bool IsEnabled(LogLevel logLevel) {
			return logLevel>=MinLevel;
		}

		public void Info(string text, Exception exception=null)
		{
			Log<string>(LogLevel.Information, text, exception, (s,e)=>s);
		}

		public void Warning(string text, Exception exception=null)
		{
			Log<string>(LogLevel.Warning, text, exception, (s,e)=>s);
		}

		public void Error(string text, Exception exception=null)
		{
			Log<string>(LogLevel.Error, text, exception, (s,e)=>s);
		}

		private string GetShortLogLevel(LogLevel logLevel) {
			switch (logLevel) {
				case LogLevel.Trace:
					return "TRCE";
				case LogLevel.Debug:
					return "DBUG";
				case LogLevel.Information:
					return "INFO";
				case LogLevel.Warning:
					return "WARN";
				case LogLevel.Error:
					return "FAIL";
				case LogLevel.Critical:
					return "CRIT";
			}
			return logLevel.ToString().ToUpper();
		}

		private void Log<TState>(LogLevel logLevel, TState state, Exception exception, Func<TState, Exception, string> formatter) 
		{
			if (formatter == null) {
				throw new ArgumentNullException(nameof(formatter));
			}

			if (!IsEnabled(logLevel)) {
				return;
			}

			string message = null;
			if (null != formatter) {
				message = formatter(state, exception);
			}

			// default formatting logic
			var logBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(message))
			{
				logBuilder.Append(DateTime.Now.ToString("o"));
				logBuilder.Append("  ");
				logBuilder.Append(GetShortLogLevel(logLevel));
				logBuilder.Append("  [");
				logBuilder.Append("Bot");
				logBuilder.Append("] ");
				logBuilder.AppendLine(message);
			}

			if (exception != null) {
				// exception message
				logBuilder.AppendLine(exception.ToString());
			}

			lock(_sync)
			{
				File.AppendAllText(this._filePath, logBuilder.ToString());
			}
		}
	}
}
