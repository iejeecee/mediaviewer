#pragma once

namespace imageviewer
{
	public ref class HRTimer 
	{
	public:
		QueueTimerException(String ^message) : Exception(message)
		{
		}

		QueueTimerException(String ^message, Exception ^innerException) : Exception(message, innerException)
		{
		}
	};
}