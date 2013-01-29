#pragma once
#include "InfoIcon.h"

namespace imageviewer {

using namespace System::Collections::Generic;

public ref class ImageGridItem 
{
public:

	enum class InfoIconModes {
		SHOW_ALL_ICONS,
		DEFAULT_ICONS_ONLY,
		CUSTOM_ICONS_ONLY,
		DISABLE_ICONS
	};

private:

	String ^imageLocation;
	Object ^data;
	String ^caption;
	bool isSelected;
	ContextMenuStrip ^contextMenu;
	List<InfoIcon ^> ^infoIcon;

	InfoIconModes infoIconMode;

public:

	ImageGridItem(String ^imageLocation) 
	{

		this->imageLocation = imageLocation;
		caption = L"";
		data = nullptr;
		contextMenu = nullptr;
		infoIcon = gcnew List<imageviewer::InfoIcon ^>();

		infoIconMode = InfoIconModes::CUSTOM_ICONS_ONLY;
	}

	property String ^ImageLocation {

		String ^get() {

			return(imageLocation);
		}

		void set(String ^imageLocation) {

			this->imageLocation = imageLocation;
		}

	}

	property Object ^Data {

		Object ^get() {

			return(data);
		}

		void set(Object ^data) {

			this->data = data;
		}

	}

	property String ^Caption {

		String ^get() {

			return(caption);
		}

		void set(String ^caption) {

			this->caption = caption;
		}

	}

	property bool IsSelected {

		bool get() {

			return(isSelected);
		}

		void set(bool isSelected) {

			this->isSelected = isSelected;
		}

	}
	
	property ContextMenuStrip ^ContextMenu {

		ContextMenuStrip ^get() {

			return(contextMenu);
		}

		void set(ContextMenuStrip ^contextMenu) {

			this->contextMenu = contextMenu;
		}

		
	}

	property InfoIconModes InfoIconMode {

		InfoIconModes get() {

			return(infoIconMode);
		}

		void set(InfoIconModes infoIconMode) {

			this->infoIconMode = infoIconMode;
		}

	}

	property List<InfoIcon ^> ^InfoIcon {

		List<imageviewer::InfoIcon ^> ^get() {

			return(infoIcon);
		}

		void set(List<imageviewer::InfoIcon ^> ^infoIcon) {

			this->infoIcon = infoIcon;
		}

	}

};

}