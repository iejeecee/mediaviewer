using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace MediaViewer.Logging
{
    class VisualAppender : log4net.Appender.AppenderSkeleton
	{
	
		private LogWindow logWindow;

		private int infoLevel;
		private int debugLevel;
		private int errorLevel;
		private int warningLevel;
		private int fatalLevel;

		protected override void Append(log4net.Core.LoggingEvent loggingEvent) 
		{

			LogWindow.LogLevel logLevel;

			if(loggingEvent.Level.Value == infoLevel) {

				logLevel = LogWindow.LogLevel.INFO;

			} else if(loggingEvent.Level.Value == debugLevel) {

				logLevel = LogWindow.LogLevel.DEBUG;

			} else if(loggingEvent.Level.Value == errorLevel) {

				logLevel = LogWindow.LogLevel.ERROR;

			} else if(loggingEvent.Level.Value == warningLevel) {

				logLevel = LogWindow.LogLevel.WARNING;

			} else if(loggingEvent.Level.Value == fatalLevel) {

				logLevel = LogWindow.LogLevel.FATAL;
			
			} else {

				logLevel = LogWindow.LogLevel.UNKNOWN;
			}

			string logString = RenderLoggingEvent(loggingEvent);

			logWindow.append(logLevel, loggingEvent.LocationInformation.ClassName + " - " + logString);
		}

	

		public VisualAppender() {

			log4net.Repository.ILoggerRepository repository = LogManager.GetRepository();

			debugLevel = repository.LevelMap["DEBUG"].Value;
			warningLevel = repository.LevelMap["WARN"].Value;
			infoLevel = repository.LevelMap["INFO"].Value;
			errorLevel = repository.LevelMap["ERROR"].Value;
			fatalLevel = repository.LevelMap["FATAL"].Value;

			logWindow = new LogWindow();
		}
	

		 public LogWindow LogWindow
		{
			get {

				return(logWindow);
			}
		}

	}
}
