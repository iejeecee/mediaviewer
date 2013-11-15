#pragma once

namespace imageviewer {

public ref class ResizeUpdateEventArgs : public System::EventArgs
{
private:
	int _width;
	int _height;

public:
	ResizeUpdateEventArgs(int width, int height) {

		_width = width;
		_height = height;
	}

	property int width {

		int get() { return(_width); }
		void set(int width) { _width = width; }
	}

	property int height {

		int get() { return(_height); }
		void set(int height) { _height = height; }
	}
};

}