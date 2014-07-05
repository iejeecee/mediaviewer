//http://www.codeproject.com/Articles/28969/HowTo-Export-C-classes-from-a-DLL#CppMatureApproach
// ------NOTE-------
// To debug native code in Visual Studio 2012 check the following option:
// MediaViewer->Properties->Debug->Enable Native Code Debugging
// To be able to dereference native variables in the debugger make sure the following is also checked
// Debug->Options and Settings->General->Managed C++ Compatibilty Mode

#pragma once

#ifdef XMPLIB_IMPORTS
#define XMPDLL_API __declspec(dllimport)
#else
#define XMPDLL_API __declspec(dllexport)
#endif

#ifndef WINAPI
#define WINAPI __stdcall
#endif

#ifdef __cplusplus
#   define EXTERN_C     extern "C"
#else
#   define EXTERN_C
#endif // __cplusplus

#ifndef WIN_ENV
#define WIN_ENV
#endif

#include <string>
#include <vector>
#include "XMP_Const.h"

namespace XMPDLL {

	class XMPProperty
	{
	public:
		std::string schemaNS;
		std::string propPath;
		std::string propVal;

	};

	class XMPFile
	{
	public:
				
		virtual bool open(const char *filenameUTF8, XMP_OptionBits options) = 0;
		virtual void release() = 0;

		virtual bool dumpToDisk(const std::string &filename) const = 0;

		virtual bool canPutXMP() = 0;
		virtual void putXMP() = 0;
		
		virtual bool getProperty(const std::string &nameSpace, 
			const std::string &propName, 
			std::string &propValue) const = 0;

		virtual bool getProperty_Date(const std::string &nameSpace,
						    const std::string &propName,
						    XMP_DateTime &propValue) const = 0;

		virtual bool getProperty_Bool(const std::string &nameSpace,
						    const std::string &propName,
						    bool &propValue) const = 0;

		virtual bool getProperty_Float(const std::string &nameSpace,
						    const std::string &propName,
						    double &propValue) const = 0;

		virtual bool getProperty_Int(const std::string &nameSpace,
						    const std::string &propName,
						    XMP_Int32 &propValue) const = 0;

		virtual bool getProperty_Int64(const std::string &nameSpace,
						    const std::string &propName,
						    XMP_Int64 &propValue) const = 0;

		virtual void deleteProperty(const std::string &nameSpace, 
			const std::string &propName) = 0;

		virtual void setProperty_Date(const std::string &nameSpace, 
			const std::string &propName, 
			const XMP_DateTime &propValue) = 0;

		virtual void setProperty(const std::string &nameSpace, 
			const std::string &propName, 
			const std::string &propValue, XMP_OptionBits options = 0) = 0;

		virtual bool doesPropertyExists(const std::string &nameSpace, 
			const std::string &propName) const = 0;

		virtual int countArrayItems(const std::string &nameSpace, 
			const std::string &arrayName) const = 0;

		virtual bool getArrayItem(const std::string &nameSpace, 
			const std::string &arrayName, 
			int item, 
			std::string &itemValue) const = 0;

		virtual bool doesArrayItemExist(const std::string &nameSpace, 
			const std::string &arrayName, 
			int item) const = 0;

		virtual void setArrayItem(const std::string &nameSpace, 
			const std::string &arrayName, 
			int item, 
			const std::string &itemValue,
			XMP_OptionBits options = 0) = 0;

		virtual void appendArrayItem(const std::string &nameSpace, 
			const std::string &arrayName, 
			XMP_OptionBits arrayOptions, 
			const char *itemValue,
			XMP_OptionBits options = 0) = 0;		

		virtual void deleteArrayItem(const std::string &nameSpace, 
			const std::string &arrayName, 
			int item) = 0;

		virtual bool getStructField(const std::string &nameSpace,
			const std::string &structName,
			const std::string &fieldNameSpace,
			const std::string &fieldName,
			std::string &fieldValue) const = 0;

		virtual void setStructField(const std::string &nameSpace,
			const std::string &structName,
			const std::string &fieldNameSpace,
			const std::string &fieldName,
			const std::string &fieldValue,
			XMP_OptionBits options = 0) = 0;

		virtual bool getLocalizedText(const std::string &nameSpace, 
			const std::string &textName, 
			const std::string &genericLang, 
			const std::string &specificLang, 
			std::string &itemValue) const = 0;

		virtual void setLocalizedText(const std::string &nameSpace, 
			const std::string &textName, 
			const std::string &genericLang, 
			const std::string &specificLang, 
			const std::string &itemValue) = 0;

		virtual void iterate(XMP_OptionBits options, std::vector<XMPProperty> &properties) const = 0;
		virtual void iterate(const std::string &nameSpace, std::vector<XMPProperty> &properties) const = 0;
		virtual void iterate(const std::string &nameSpace, XMP_OptionBits options, std::vector<XMPProperty> &properties) const = 0;
		
		virtual void catenateArrayItems(const std::string &nameSpace,
			const std::string &arrayName,
			const std::string &separator,
			const std::string &quotes,
			XMP_OptionBits options,
			std::string &catedStr) const = 0;
		
		 
	};

	
	EXTERN_C XMPDLL_API XMPFile* WINAPI newXMPFile(XMPFiles_ErrorCallbackProc errorProc = NULL, XMP_ProgressReportProc progressProc = NULL, void *context = NULL);

	EXTERN_C XMPDLL_API void WINAPI convertToDate(const std::string &dateString, 
		XMP_DateTime &date);

	EXTERN_C XMPDLL_API void WINAPI composeArrayItemPath(const std::string &nameSpace, 
		const std::string &arrayName, 
		int itemIndex, 
		std::string &fullPath);

	EXTERN_C XMPDLL_API void WINAPI composeStructFieldPath(const std::string &nameSpace, 
		const std::string &structName, 
		const std::string &fieldNameSpace, 
		const std::string &fieldName, 
		std::string &fullPath);

	EXTERN_C XMPDLL_API void WINAPI encodeToBase64(const std::string &rawStr,
		std::string &encodedStr); 

	EXTERN_C XMPDLL_API void WINAPI decodeFromBase64(const std::string &encodedStr,
		std::string &rawStr); 

	EXTERN_C XMPDLL_API void WINAPI getVersionInfo(XMP_VersionInfo *versionInfo);

}