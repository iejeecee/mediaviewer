// XMPLib.cpp : Defines the entry point for the console application.
//

#include "stdafx.h"
#include "XMPLib.h"

// =================================================================================================
// Copyright 2008 Adobe Systems Incorporated
// All Rights Reserved.
//
// NOTICE:  Adobe permits you to use, modify, and distribute this file in accordance with the terms
// of the Adobe license agreement accompanying it.
// =================================================================================================

/**
* Tutorial solution for the Walkthrough 1 in the XMP Programmers Guide, Opening files and reading XMP.
* Demonstrates the basic use of the XMPFiles and XMPCore components, obtaining read-only XMP from a file 
* and examining it through the XMP object.
*/



// Must be defined to instantiate template classes
#define TXMP_STRING_TYPE std::string 

// Must be defined to give access to XMPFiles
#define XMP_INCLUDE_XMPFILES 1 

// Ensure XMP templates are instantiated
#include "XMP.incl_cpp"

// Provide access to the API
#include "XMP.hpp"

#include <iostream>
#include <fstream>

using namespace std; 

static XMP_Status dumpCallback ( void * refCon, XMP_StringPtr outStr, XMP_StringLen outLen )
{
	XMP_Status	status	= 0; 
	size_t		count;
	FILE *		outFile	= static_cast < FILE * > ( refCon );
	
	count = fwrite ( outStr, 1, outLen, outFile );
	if ( count != outLen ) status = errno;
	return status;
	
}	

namespace XMPLib {

	class XMPFileImpl : public XMPFile 
	{
	private:

		SXMPMeta meta;
		SXMPFiles myFile;
	
	public:

		bool open(const std::string &filename, XMP_OptionBits options) {

			if(filename.empty()) 
			{
				return(false);
			}

			if(!SXMPMeta::Initialize())
			{
				return(false);
			}

			//XMP_OptionBits options = 0;

			// Must initialize SXMPFiles before we use it
			if (!SXMPFiles::Initialize (0) )
			{
				return(false);
			}

			try
			{
				// Options to open the file with - read only and use a file handler
				XMP_OptionBits opts = options | kXMPFiles_OpenUseSmartHandler;

				bool ok;							

				// First we try and open the file
				ok = myFile.OpenFile(filename, kXMP_UnknownFile, opts);
				if(!ok)
				{
					// Now try using packet scanning
					opts = options | kXMPFiles_OpenUsePacketScanning;			
					ok = myFile.OpenFile(filename, kXMP_UnknownFile, opts);
				}

				if(ok)
				{			
					myFile.GetXMP(&meta);
				    return(true);
				}

			} catch(XMP_Error & e)
			{			
				cout << "ERROR: " << e.GetErrMsg() << endl;
			}

			myFile.CloseFile();
			return(false);
		}

		bool dumpToDisk(const std::string &filename) const {

			FILE *outFile = NULL;

			fopen_s(&outFile, filename.c_str(), "w");

			if(outFile == NULL) return(false);

			XMP_Status result = meta.DumpObject( dumpCallback, outFile );

			fclose(outFile);

			return(result == 0 ? true : false);
		}

		bool canPutXMP(void) {

			return(myFile.CanPutXMP(meta));
		}

		void putXMP(void) {

			myFile.PutXMP(meta);
			myFile.CloseFile();
		}

		bool getProperty(const std::string &nameSpace, const std::string &name, std::string &value) const {

			bool exists = meta.GetProperty(nameSpace.c_str(), name.c_str(), &value, NULL);

			if(exists == false) {

				value.clear();
			}

			return(exists);

		}

		void deleteProperty(const std::string &nameSpace, const std::string &propName) {

			meta.DeleteProperty(nameSpace.c_str(), propName.c_str());
		}


		bool getProperty_Date(const std::string &nameSpace, const std::string &propName,
			XMP_DateTime &propValue) const
		{
			bool exists = meta.GetProperty_Date(nameSpace.c_str(), propName.c_str(), &propValue, NULL);

			if(exists == false) {

				XMP_DateTime temp;
				propValue = temp;
			}

			return(exists);

		}

		void setProperty(const std::string &nameSpace, const std::string &name, const std::string &value, XMP_OptionBits options) {

			meta.SetProperty(nameSpace.c_str(), name.c_str(), value.c_str(), options);
		}

		void setProperty_Date(const std::string &nameSpace, const std::string &propName, const XMP_DateTime &propValue) {

			meta.SetProperty_Date(nameSpace.c_str(), propName.c_str(), propValue, NULL);
		}

		bool doesPropertyExists(const std::string &nameSpace, const std::string &propName) const {

			bool exists = meta.DoesPropertyExist(nameSpace.c_str(), propName.c_str());
			
			return(exists);
		}

		bool getLocalizedText(const std::string &nameSpace, const std::string &textName, const std::string &genericLang, const std::string &specificLang, std::string &itemValue) const
		{
			bool exists = meta.GetLocalizedText(nameSpace.c_str(), textName.c_str(), genericLang.c_str(), specificLang.c_str(), NULL, &itemValue, NULL);

			if(exists == false) {

				itemValue.clear();
			}

			return(exists);

		}

		void setLocalizedText(const std::string &nameSpace, const std::string &textName, const std::string &genericLang, const std::string &specificLang, const std::string &itemValue) {

			meta.SetLocalizedText(nameSpace.c_str(), textName.c_str(), genericLang.c_str(), specificLang.c_str(), itemValue.c_str(), NULL);
		}

		int countArrayItems(const std::string &nameSpace, const std::string &arrayName) const {

			return(meta.CountArrayItems(nameSpace.c_str(), arrayName.c_str()));
		}
		
		bool getArrayItem(const std::string &nameSpace, const std::string &arrayName, int item, std::string &itemValue) const {

			bool exists = meta.GetArrayItem(nameSpace.c_str(), arrayName.c_str(), item, &itemValue, NULL);

			if(exists == false) {

				itemValue.clear();
			}

			return(exists);
			
		}
		
		bool doesArrayItemExist(const std::string &nameSpace, const std::string &arrayName, int item) const 
		{
			bool exists = meta.DoesArrayItemExist(nameSpace.c_str(), arrayName.c_str(), item);

			return(exists);
		}

		void setArrayItem(const std::string &nameSpace, const std::string &arrayName, int item, const std::string &itemValue, XMP_OptionBits options) 
		{
			meta.SetArrayItem(nameSpace.c_str(), arrayName.c_str(), item, itemValue.c_str(), options);
		}

		void appendArrayItem(const std::string &nameSpace, const std::string &arrayName, XMP_OptionBits arrayOptions, const char *itemValue, XMP_OptionBits options) {

			meta.AppendArrayItem(nameSpace.c_str(), arrayName.c_str(), arrayOptions, itemValue, options);
		
		
		}

		void deleteArrayItem(const std::string &nameSpace, const std::string &arrayName, int index) {

			meta.DeleteArrayItem(nameSpace.c_str(), arrayName.c_str(), index);
				
		}

		bool getStructField(const std::string &nameSpace,
			const std::string &structName,
			const std::string &fieldNameSpace,
			const std::string &fieldName,
			std::string &fieldValue) const
		{

			bool exists = meta.GetStructField(nameSpace.c_str(), structName.c_str(), fieldNameSpace.c_str(), fieldName.c_str(), &fieldValue, 0);

			return(exists);
		}

		void setStructField(const std::string &nameSpace,
			const std::string &structName,
			const std::string &fieldNameSpace,
			const std::string &fieldName,
			const std::string &fieldValue,
			XMP_OptionBits options = 0) 
		{
			meta.SetStructField(nameSpace.c_str(), structName.c_str(), fieldNameSpace.c_str(), fieldName.c_str(), fieldValue, options);
		}
		 
		void iterate(XMP_OptionBits options, std::vector<XMPProperty> &properties) const {

			properties.clear();

			SXMPIterator iter(meta, options);

			XMPProperty p;
		
			while(iter.Next(&p.schemaNS, &p.propPath, &p.propVal))
			{
				properties.push_back(p);
			}

		}

		void iterate(const std::string &nameSpace, std::vector<XMPProperty> &properties) const {

			properties.clear();

			SXMPIterator iter(meta, nameSpace.c_str());

			XMPProperty p;
		
			while(iter.Next(&p.schemaNS, &p.propPath, &p.propVal))
			{

				properties.push_back(p);
			}

		}

		void iterate(const std::string &nameSpace, XMP_OptionBits options, std::vector<XMPProperty> &properties) const {

			properties.clear();

			SXMPIterator iter(meta, nameSpace.c_str(), options);

			XMPProperty p;
		
			while(iter.Next(&p.schemaNS, &p.propPath, &p.propVal))
			{

				properties.push_back(p);
			}

		}

		void catenateArrayItems(const std::string &nameSpace,
			const std::string &arrayName,
			const std::string &separator,
			const std::string &quotes,
			XMP_OptionBits options,
			std::string &catedStr) const
		{
			
			TXMPUtils<std::string>::CatenateArrayItems(meta, nameSpace.c_str(),
				arrayName.c_str(), separator.c_str(), quotes.c_str(), options, &catedStr);

		}


		void release() {

			myFile.CloseFile();
			delete this;
		}	

		
	};

	XMPLIB_API XMPFile* WINAPI newXMPFile(void)
	{
		return new XMPFileImpl();
	}

	XMPLIB_API void WINAPI convertToDate(const std::string &dateString, XMP_DateTime &date) {

		TXMPUtils<std::string>::ConvertToDate(dateString, &date);
	}

	XMPLIB_API void WINAPI composeArrayItemPath(const std::string &nameSpace, 
		const std::string &arrayName, 
		int itemIndex, 
		std::string &fullPath) 
	{
		TXMPUtils<std::string>::ComposeArrayItemPath(nameSpace.c_str(), arrayName.c_str(), itemIndex, &fullPath);
	}

	XMPLIB_API void WINAPI composeStructFieldPath(const std::string &nameSpace, 
		const std::string &structName, 
		const std::string &fieldNameSpace, 
		const std::string &fieldName, 
		std::string &fullPath)
	{
		TXMPUtils<std::string>::ComposeStructFieldPath(nameSpace.c_str(), structName.c_str(), fieldNameSpace.c_str(), fieldName.c_str(), &fullPath);
	}

	XMPLIB_API void WINAPI encodeToBase64(const std::string &rawStr,
		std::string &encodedStr) 	
	{
		TXMPUtils<std::string>::EncodeToBase64(rawStr, &encodedStr);
	}

	XMPLIB_API void WINAPI decodeFromBase64(const std::string &encodedStr,
		std::string &rawStr) 	
	{
		TXMPUtils<std::string>::DecodeFromBase64(encodedStr, &rawStr);
	}

}
