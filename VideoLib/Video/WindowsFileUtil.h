#pragma once
#include <string>
#include <fstream>
#include <iostream>

namespace VideoLib {

class WindowsFileUtil {

public:

	static bool splitPath(const std::string &path, 
		std::string *drive = NULL,
		std::string *dir = NULL,
		std::string *filename = NULL,
		std::string *extension = NULL)
	{

		if(path.find("://") != std::string::npos) {

			// input is a url
			return(false);
		}

		char driveBuf[_MAX_DRIVE];
		char dirBuf[_MAX_DIR];
		char filenameBuf[_MAX_FNAME];
		char extensionBuf[_MAX_EXT];

		errno_t error = _splitpath_s(
			path.c_str(), 
			driveBuf, _MAX_DRIVE, 
			dirBuf, _MAX_DIR, 
			filenameBuf, _MAX_FNAME, 
			extensionBuf, _MAX_EXT);

		if(drive != NULL) {
			*drive = std::string(driveBuf);
		}
		if(dir != NULL) {
			*dir = std::string(dirBuf);
		}
		if(filename != NULL) {
			*filename = std::string(filenameBuf);
		}
		if(extension != NULL) {
			*extension = std::string(extensionBuf);
		}

		return true;
		
	}

	static bool fileExists(const std::string &fileName) {

		std::ifstream file(fileName.c_str());

		if(file.fail()) {

			return(false);

		} else {

			file.close();
			return(true);
		}

	}

};

}