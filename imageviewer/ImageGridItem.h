#pragma once

namespace imageviewer {

public ref class ImageGridItem 
{

	String ^_imageLocation;
	Object ^_data;
	String ^_caption;
	bool _isSelected;
	ContextMenuStrip ^_contextMenu;

public:

	ImageGridItem(String ^imageLocation) 
	{

		_imageLocation = imageLocation;
		caption = L"";
		data = nullptr;
		contextMenu = nullptr;
	}

	property String ^imageLocation {

		String ^get() {

			return(_imageLocation);
		}

		void set(String ^imageLocation) {

			_imageLocation = imageLocation;
		}

	}

	property Object ^data {

		Object ^get() {

			return(_data);
		}

		void set(Object ^data) {

			_data = data;
		}

	}

	property String ^caption {

		String ^get() {

			return(_caption);
		}

		void set(String ^caption) {

			_caption = caption;
		}

	}

	property bool isSelected {

		bool get() {

			return(_isSelected);
		}

		void set(bool isSelected) {

			_isSelected = isSelected;
		}

	}
	
	property ContextMenuStrip ^contextMenu {

		ContextMenuStrip ^get() {

			return(_contextMenu);
		}

		void set(ContextMenuStrip ^contextMenu) {

			_contextMenu = contextMenu;
		}

		
	}

};

}