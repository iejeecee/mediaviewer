using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MediaViewer.Infrastructure.Logging
{
    public class VisualAppender : log4net.Appender.AppenderSkeleton
	{
        LogViewModel logViewModel;

        public LogViewModel LogViewModel
        {
            get { return logViewModel; }
        }

		private int infoLevel;
		private int debugLevel;
		private int errorLevel;
		private int warningLevel;
		private int fatalLevel;

		protected override void Append(log4net.Core.LoggingEvent loggingEvent) 
		{
			LogMessageModel.LogLevel logLevel;

			if(loggingEvent.Level.Value == infoLevel) {

				logLevel = LogMessageModel.LogLevel.INFO;

			} else if(loggingEvent.Level.Value == debugLevel) {

				logLevel = LogMessageModel.LogLevel.DEBUG;

			} else if(loggingEvent.Level.Value == errorLevel) {

				logLevel = LogMessageModel.LogLevel.ERROR;

			} else if(loggingEvent.Level.Value == warningLevel) {

				logLevel = LogMessageModel.LogLevel.WARNING;

			} else if(loggingEvent.Level.Value == fatalLevel) {

				logLevel = LogMessageModel.LogLevel.FATAL;
			
			} else {

				logLevel = LogMessageModel.LogLevel.UNKNOWN;
			}

			string logString = RenderLoggingEvent(loggingEvent);

            LogMessageModel message = new LogMessageModel(logLevel, logString);

            logViewModel.addMessage(message); 
		}
	
		public VisualAppender() {

			log4net.Repository.ILoggerRepository repository = LogManager.GetRepository();
            
			debugLevel = repository.LevelMap["DEBUG"].Value;
			warningLevel = repository.LevelMap["WARN"].Value;
			infoLevel = repository.LevelMap["INFO"].Value;
			errorLevel = repository.LevelMap["ERROR"].Value;
			fatalLevel = repository.LevelMap["FATAL"].Value;

			logViewModel = new LogViewModel();
		}
	

		public LogMessageModel LogMessageModel
		{
			get {

				return(LogMessageModel);
			}
		}
        
	}
}
