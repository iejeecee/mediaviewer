#pragma once

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Text::RegularExpressions;
using namespace System::Collections::Generic;


public ref class Util 
{

public: 
	
	static void DebugOut(String ^string) {

		System::Diagnostics::Debug::Print(string);
	}

	static void DebugOut(Object ^object) {

		System::Diagnostics::Debug::Print(object->ToString());
	}

	static bool isUrl(String ^string) 
	{

			if(string->StartsWith(L"http://") || string->StartsWith(L"https://")) {

				return(true);

			} else {

				return(false);
			}
	}

	static String ^getProperDirectoryCapitalization(DirectoryInfo ^dirInfo)
	{
		DirectoryInfo ^parentDirInfo = dirInfo->Parent;
		if(parentDirInfo == nullptr) {

			return dirInfo->Name;
		}

		return Path::Combine(getProperDirectoryCapitalization(parentDirInfo),
			parentDirInfo->GetDirectories(dirInfo->Name)[0]->Name);
	}

	static String ^getProperFilePathCapitalization(String ^filename)
	{
		FileInfo ^fileInfo = gcnew FileInfo(filename);
		DirectoryInfo ^dirInfo = fileInfo->Directory;

		String ^result = Path::Combine(getProperDirectoryCapitalization(dirInfo),
			dirInfo->GetFiles(fileInfo->Name)[0]->Name);

		return(Char::ToUpper(result[0]) + result->Substring(1));
	}


	static String ^getPathWithoutFileName(String ^fullPath) {

		String ^fileName = System::IO::Path::GetFileName(fullPath);

		if(String::IsNullOrEmpty(fileName)) return(fullPath);

		return(fullPath->Remove(fullPath->Length - fileName->Length - 1));
	}

	static String ^removeIllegalCharsFromFileName(String  ^fileName) {

		String ^regexSearch = gcnew String(Path::GetInvalidFileNameChars()) + gcnew String(Path::GetInvalidPathChars());
		Regex ^r = gcnew Regex(String::Format("[{0}]", Regex::Escape(regexSearch)));
		return( r->Replace(fileName, "") );

	}

	static String ^formatTimeSeconds(int totalSeconds) {

		int seconds = int(totalSeconds % 60);
		int minutes = int((totalSeconds / 60) % 60);
		int hours = int(totalSeconds / 3600);

		String ^hoursStr = "";
		
		if(hours != 0) {

			hoursStr = hours.ToString() + ":";
		} 

		String ^output = hoursStr +
			minutes.ToString("00") + ":" +
			seconds.ToString("00");

		return(output);

	}

	static String ^formatSizeBytes(__int64 sizeBytes) {

		__int64 GB = 1073741824;
		__int64 MB = 1048576;
		__int64 KB = 1024;
		String ^output;

		if(sizeBytes > GB) {

			output = (sizeBytes / double(GB)).ToString("0.00") + " GB";

		} else if(sizeBytes > MB) {

			output = (sizeBytes / double(MB)).ToString("0.00") + " MB";

		} else if(sizeBytes > KB) {

			output = (sizeBytes / double(KB)).ToString("0") + " KB";

		} else {

			output = sizeBytes.ToString() + " Bytes";
		}

		return(output);
	}

	template<class T>
	static T clamp(T val, T min, T max) {

		if(val < min) val = min;
		else if(val > max) val = max;

		return(val);
	}

	template<class T>
	static T lerp(double val, T min, T max) {

		val = clamp<double>(val, 0, 1);
		
		return(T((1-val) * min + val * max));
	}

	template<class T>
	static double invlerp(T val, T min, T max) {

		double result = (val - min) / double(max - min);
			
		return(result);
	}

	generic <typename T>
	static bool listSortAndCompare(List<T> ^a, List<T> ^b) {

		if(a->Count != b->Count) return(false);

		a->Sort();
		b->Sort();

		for(int i = 0; i < a->Count; i++) {

			if(!a[i]->Equals(b[i])) {
				
				return(false);
			}
		}

		return(true);
	}

};

}