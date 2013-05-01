#pragma once

#include "LogForm.h"

namespace imageviewer 
{
	using namespace System;
	using namespace log4net;

	public ref class VisualAppender : Appender::AppenderSkeleton
	{
	private:

		LogForm ^logForm;

		int infoLevel;
		int debugLevel;
		int errorLevel;
		int warningLevel;
		int fatalLevel;

	protected:
		
		virtual void Append(Core::LoggingEvent ^loggingEvent) override
		{

			LogForm::LogLevel logLevel;

			if(loggingEvent->Level->Value == infoLevel) {

				logLevel = LogForm::LogLevel::INFO;

			} else if(loggingEvent->Level->Value == debugLevel) {

				logLevel = LogForm::LogLevel::DEBUG;

			} else if(loggingEvent->Level->Value == errorLevel) {

				logLevel = LogForm::LogLevel::ERROR;

			} else if(loggingEvent->Level->Value == warningLevel) {

				logLevel = LogForm::LogLevel::WARNING;

			} else if(loggingEvent->Level->Value == fatalLevel) {

				logLevel = LogForm::LogLevel::FATAL;
			
			} else {

				logLevel = LogForm::LogLevel::UNKNOWN;
			}

			String ^logString = RenderLoggingEvent(loggingEvent);

			logForm->append(logLevel, loggingEvent->LocationInformation->ClassName + " - " + logString);
		}

	public:

		VisualAppender() {

			Repository::ILoggerRepository ^repository = LogManager::GetRepository();

			debugLevel = repository->LevelMap["DEBUG"]->Value;
			warningLevel = repository->LevelMap["WARN"]->Value;
			infoLevel = repository->LevelMap["INFO"]->Value;
			errorLevel = repository->LevelMap["ERROR"]->Value;
			fatalLevel = repository->LevelMap["FATAL"]->Value;

			logForm = gcnew LogForm();
		}
	

		property LogForm ^Form
		{
			LogForm ^get() {

				return(logForm);
			}
		}

	};

}