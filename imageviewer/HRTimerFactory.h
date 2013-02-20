#pragma once
#include "DefaultTimer.h"
#include "MultiMediaTimer.h"

namespace imageviewer
{
	public ref class HRTimerFactory 
	{
	
	public:

	enum class TimerType
	{
		DEFAULT,
		MULTI_MEDIA,
		TIMER_QUEUE
	};

	static HRTimer ^create(TimerType type) {

		HRTimer ^timer = nullptr;

		switch(type) {

			case TimerType::DEFAULT: 
				{
					timer = gcnew DefaultTimer();
					break;
				}
			case TimerType::MULTI_MEDIA: 
				{

					timer = gcnew MultiMediaTimer();
					break;
				}
			case TimerType::TIMER_QUEUE:
				{

					break;
				}

		}

		return(timer);
	}

		
	};
}