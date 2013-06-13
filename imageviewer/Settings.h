#pragma once

namespace imageviewer {

using namespace System;
using namespace Microsoft::Win32;

public ref class Settings {
	
public:

	enum class VarName {
		VIDEO_VOLUME,
		VIDEO_MUTED,
		VIDEO_SCREENSHOT_FILE_TYPE,
		VIDEO_SCREENSHOT_DIRECTORY,
		VIDEO_SCREENSHOT_METADATA
	};


private:

	static Dictionary<String ^, String ^> ^settings;

	static String ^getKeyName() {

		 String^ userRoot = "HKEY_CURRENT_USER";
		 String^ appName = "MediaViewer";
		 String^ registryVersion = "1.0";
		 String^ keyName = userRoot + "\\" + appName + "\\" + registryVersion;

		 return(keyName);
	}


public:

	static Settings() {

		settings = gcnew Dictionary<String ^, String ^>();

		load();
	}

	static void setDefaults() {

		setVar(VarName::VIDEO_VOLUME, 1);
		setVar(VarName::VIDEO_MUTED, false);
		setVar(VarName::VIDEO_SCREENSHOT_FILE_TYPE, "png");
		setVar(VarName::VIDEO_SCREENSHOT_DIRECTORY, "default");
		setVar(VarName::VIDEO_SCREENSHOT_METADATA, "true");
	}

	static void load() {

		settings->Clear();

		for each(VarName valueName in Enum::GetValues(VarName::typeid))
		{
			String ^value = (String ^)Registry::GetValue(getKeyName(), valueName.ToString(), nullptr);

			if(value == nullptr) {

				setDefaults();
				return;
			}

			setVar(valueName, value);
		}
	}

	static void save() {

		for each(VarName valueName in Enum::GetValues(VarName::typeid))
		{
			Registry::SetValue(getKeyName(), valueName.ToString(), getVar(valueName));
		}
	}

	static String ^getVar(VarName name) {

		return(settings[name.ToString()]);
	}

	static void setVar(VarName name, Object ^value) {

		settings[name.ToString()] = value->ToString();
	}
};


}