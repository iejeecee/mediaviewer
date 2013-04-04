#pragma once

#include "XMPDLL.h"
//#include "MetaDataProperty.h"
//#include "MetaDataTree.h"

namespace XMPLib {

	using namespace System;
	using namespace System::Collections::Generic;

	public ref class MetaDataProperty 
	{
	public:

		String ^nameSpace;
		String ^path;
		String ^value;

		MetaDataProperty();
		MetaDataProperty(const MetaDataProperty ^p);

		virtual bool Equals(Object ^obj) override;

	};

	public ref class MetaData
	{

	public:

		enum class LogLevel 
		{
			ERROR,
			WARNING,
			INFO
		};

		delegate void LogCallbackDelegate(LogLevel level, String ^message);

	private:

		XMPDLL::XMPFile *xmpFile;

		static LogCallbackDelegate ^logCallback;

		static void log(LogLevel level, String ^message);

	public:

		static MetaData() {

			logCallback = nullptr;
		}

		MetaData();
		~MetaData();

		bool open(String ^filename, XMP_OptionBits options); 

		bool dumpToDisk(String ^filename);

		bool canPutXMP();
		void putXMP();

		bool doesPropertyExists(String ^nameSpace, String ^propName);
		bool getProperty(String ^nameSpace, String ^propName, String^ %propValue);
		void deleteProperty(String ^nameSpace, String ^propName);
		bool getProperty_Date(String ^nameSpace, String ^propName, DateTime %propValue);
		void setProperty(String ^nameSpace, String ^propName, String ^propValue, XMP_OptionBits options);
		void setProperty_Date(String ^nameSpace, String ^propName, DateTime propValue);
		
		int countArrayItems(String ^nameSpace, String ^arrayName);
		bool getArrayItem(String ^nameSpace, String ^arrayName, int item, String^ %itemValue);
		bool doesArrayItemExist(String ^nameSpace, String ^arrayName, int item);
		void setArrayItem(String ^nameSpace, String ^arrayName, int item, String ^itemValue, XMP_OptionBits options);
		void appendArrayItem(String ^nameSpace, String ^arrayName, XMP_OptionBits arrayOptions, String ^itemValue, XMP_OptionBits options);
		void deleteArrayItem(String ^nameSpace, String ^arrayName, int item);

		bool getStructField(String ^nameSpace, String ^structName, String ^fieldNameSpace, 
			String ^fieldName, String ^%fieldValue);
		void setStructField(String ^nameSpace, String ^structName, String ^fieldNameSpace, 
			String ^fieldName, String ^fieldValue, XMP_OptionBits options);

		bool getLocalizedText(String ^nameSpace, String ^textName, String ^genericLang,  String ^specificLang, String ^ %itemValue);
		void setLocalizedText(String ^nameSpace, String ^textName, String ^genericLang, String ^specificLang, String ^itemValue);
		
		//MetaDataTreeNode ^parse();

		void iterate(XMP_OptionBits options, List<MetaDataProperty ^> ^%properties);
		void iterate(String ^nameSpace, List<MetaDataProperty ^> ^%properties);
		void iterate(String ^nameSpace, XMP_OptionBits options, List<MetaDataProperty ^> ^%properties);
		
		static DateTime convertToDate(String ^dateString);

		static void composeArrayItemPath(String ^nameSpace, 
			String ^arrayName, 
			int itemIndex, 
			String ^%fullPath);

		static void composeStructFieldPath(String ^nameSpace, 
			String ^structName, 
			String ^fieldNameSpace, 
			String ^fieldName, 
			String ^%fullPath);

		static void encodeToBase64(String ^rawStr,
			String ^%encodedStr); 

		static void decodeFromBase64(String ^encodedStr,
			String ^%rawStr); 

		static void setLogCallback(LogCallbackDelegate ^callback);
	};
}
