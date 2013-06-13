#pragma once

using namespace System;
using namespace System::Threading;
using namespace System::Collections::Generic;

namespace imageviewer {

	template <class Type>
	public ref class ConcurrentList 
	{

	protected:

		Generic::List<Type> ^list;
		Object ^lock;

	public:

		ConcurrentList() {

			list = gcnew List<Type>();
			lock = gcnew System::Object();
		}

		void Add(Type t) {

			Monitor::Enter(lock);

			try
			{
				list->Add(t);
			}
			finally
			{
				Monitor::Exit(lock);
			}
		}

		Type Get(int i) {

			Monitor::Enter(lock);

			try
			{
				System::Diagnostics::Debug::Assert(i < list->Count);
				return(list[i]);
			}
			finally
			{
				Monitor::Exit(lock);
			}

		}

		void Remove(Type t) {

			Monitor::Enter(lock);

			try
			{
				if(list->Remove(t) == true) {

					// inform threads waiting for non-existence a object has been removed
					Monitor::PulseAll(lock);
				}
			}
			finally
			{
				Monitor::Exit(lock);
			}

		}

		void WaitForNonExistence(Type t)
		{
			Monitor::Enter(lock);
			try
			{
				while(list->Contains(t))
				{
					// release lock and wait until a object has been removed
					// before checking again
					Monitor::Wait(lock);
				}
			}
			finally
			{
				Monitor::Exit(lock);
			}

		}

		property int Count {

			int get() {

				Monitor::Enter(lock);

				try
				{
					return(list->Count);
				}
				finally
				{
					Monitor::Exit(lock);
				}

			}
		}
	};
}