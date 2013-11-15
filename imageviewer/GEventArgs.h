#pragma once

namespace imageviewer {

generic<typename Type> public ref class GEventArgs : public EventArgs
{

private:
	Type value;

public: 

	GEventArgs(Type value)
	{
		this->value = value;
	}

	property Type Value
	{
		Type get(void) { 
			
			return value; 
		}
	}
};

generic<typename Type> public ref class ModifiableGEventArgs : public EventArgs
{

private:
	Type value;

public: 

	ModifiableGEventArgs(Type value)
	{
		this->value = value;
	}

	property Type Value
	{
		void set(Type value) {

			this->value = value;
		}

		Type get(void) { 
			
			return value; 
		}
	}
};

}