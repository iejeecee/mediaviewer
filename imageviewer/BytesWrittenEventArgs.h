#pragma once

namespace imageviewer {

public ref class BytesWrittenEventArgs : public System::EventArgs
{
private:
	int _bytesWritten;


public:

	BytesWrittenEventArgs()
	{

		bytesWritten = 0;
	}

	property int bytesWritten {

		int get() { return(_bytesWritten); }
		void set(int bytesWritten) { _bytesWritten = bytesWritten; }
	}


};

}