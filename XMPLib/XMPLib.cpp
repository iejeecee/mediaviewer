// XMPLib.cpp : Defines the entry point for the console application.
//

#include "StdAfx.h"
#include "XMPLib.h"
#include <msclr\marshal_cppstd.h>
#include "XMPLibException.h"

#ifdef ERROR
#undef ERROR
#endif

using namespace System;
using namespace System::IO;
using namespace System::Text::RegularExpressions;
using namespace msclr::interop;
using namespace System::Runtime::InteropServices;


namespace XMPLib {

MetaDataProperty::MetaDataProperty() {

	nameSpace = "";
	path = "";
	value = "";
}

MetaDataProperty::MetaDataProperty(const MetaDataProperty ^p) {

	if(p == nullptr) return;

	nameSpace = p->nameSpace;
	path = p->path;
	value = p->value;
}

bool MetaDataProperty::Equals(Object ^obj) {

	if(obj == nullptr) return(false);

	MetaDataProperty ^b = dynamic_cast<MetaDataProperty ^>(obj);

	if(b == nullptr) return(false);

	if(b->nameSpace->Equals(nameSpace) &&
		b->path->Equals(path) &&
		b->value->Equals(value)) 
	{
		return(true);
	}

	return(false);
}


MetaData::MetaData(ErrorCallbackDelegate ^managedErrorCallback, ProgressCallbackDelegate ^managedProgressCallback) {

	this->managedErrorCallback = managedErrorCallback;
	this->managedProgressCallback = managedProgressCallback;

	XMP_ProgressReportProc nativeProgressCallback = NULL;

	if(managedProgressCallback != nullptr) {

		NativeProgressCallbackDelegate ^nativeProgressCallbackDelegate = gcnew NativeProgressCallbackDelegate(this, &MetaData::progressCallback);

		progressCallbackPtr = GCHandle::Alloc(nativeProgressCallbackDelegate);

		IntPtr ipProgressCallback = Marshal::GetFunctionPointerForDelegate(nativeProgressCallbackDelegate);
		nativeProgressCallback = static_cast<XMP_ProgressReportProc>(ipProgressCallback.ToPointer());

	}

	XMPFiles_ErrorCallbackProc nativeErrorCallback = NULL;

	if(managedErrorCallback != nullptr) {

		NativeErrorCallbackDelegate ^nativeErrorCallbackDelegate = gcnew NativeErrorCallbackDelegate(this, &MetaData::errorCallback);

		errorCallbackPtr = GCHandle::Alloc(nativeErrorCallbackDelegate);

		IntPtr ipErrorCallback = Marshal::GetFunctionPointerForDelegate(nativeErrorCallbackDelegate);
		nativeErrorCallback = static_cast<XMPFiles_ErrorCallbackProc>(ipErrorCallback.ToPointer());

	}

	xmpFile = XMPDLL::newXMPFile(nativeErrorCallback, nativeProgressCallback, NULL);

}

bool MetaData::errorCallback(void *context, XMP_StringPtr filePath, XMP_ErrorSeverity errorSeverity, XMP_Int32 cause,  XMP_StringPtr message) 
{
	bool result = true;

	if(managedErrorCallback != nullptr) {

		result = managedErrorCallback->Invoke(gcnew String(filePath), errorSeverity, cause, gcnew String(message));
	}

	return(result);
}

bool MetaData::progressCallback(void *context, float elapsedTime, float fractionDone, float secondsToGo) 
{
	bool result = true;

	if(managedProgressCallback != nullptr) {

		result = managedProgressCallback->Invoke(elapsedTime, fractionDone, secondsToGo);
	}

	return(result);

}


MetaData::~MetaData() {
	
	if(errorCallbackPtr.IsAllocated) {
		errorCallbackPtr.Free();
	}
	if(progressCallbackPtr.IsAllocated) {
		progressCallbackPtr.Free();	
	}

	if(xmpFile != nullptr) {

		xmpFile->release();
	}
}




void MetaData::setLogCallback(LogCallbackDelegate ^logCallback) {

	MetaData::logCallback = logCallback;
}

void MetaData::log(MetaData::LogLevel level, String ^message) {

	if(logCallback != nullptr) {

		logCallback->Invoke(level, message);
	}
}

bool MetaData::open(String ^filename, Consts::OpenOptions options)
{
	bool result = false;

	try {

		std::string filenameUTF8;

		WStringToUTF8(filename, filenameUTF8);
	
		result = xmpFile->open(filenameUTF8.c_str(), (XMP_OptionBits)options);

		
	} catch(XMP_Error &e) {	

		XMPLibException ^managedException = 
			gcnew XMPLibException("Cannot open " + filename + ": ", e.GetErrMsg());
		log(MetaData::LogLevel::ERROR, managedException->Message);
		
		throw managedException;
	}

	return(result);
}

bool MetaData::dumpToDisk(String ^filename) {

	bool result = false;

	try {

		result = xmpFile->dumpToDisk(marshal_as<std::string>(filename));

	} catch(XMP_Error &e) {	

		XMPLibException ^managedException = 
			gcnew XMPLibException("Error dumping metadata to disk " + filename + ": ", 
			e.GetErrMsg());
		log(MetaData::LogLevel::ERROR, managedException->Message);
		
		throw managedException;
	}

	return(result);
}

bool MetaData::canPutXMP() 
{
	return(xmpFile->canPutXMP());
}

void MetaData::putXMP()
{
	if(canPutXMP() == false) {
		
		throw gcnew XMPLibException("Writing MetaData not supported");

	} else {

		try {

			xmpFile->putXMP();

		} catch(XMP_Error &e) {			

			XMPLibException ^managedException = 
				gcnew XMPLibException("Cannot write Meta Data: ", e.GetErrMsg());
			log(MetaData::LogLevel::ERROR, managedException->Message);
			
			throw managedException;
			
		}
	}
}

void MetaData::getProperty(String ^nameSpace, String ^propName, String^ %propValue)
{
	std::string temp;

	bool result = xmpFile->getProperty(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName), 
		temp);

	if(result == true) {

		UTF8ToWString(temp, propValue);

	} else {

		propValue = nullptr;
	}
	
}

void MetaData::deleteProperty(String ^nameSpace, String ^propName) {

	xmpFile->deleteProperty(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName));
}

void MetaData::getProperty_Date(String ^nameSpace, String ^propName, Nullable<DateTime> %propValue)
{
	XMP_DateTime xmpDate;

	bool result = xmpFile->getProperty_Date(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName),
		xmpDate);
	
	try {

		if(xmpDate.hasDate && xmpDate.hasTime) {

			propValue = DateTime(xmpDate.year, xmpDate.month, xmpDate.day, xmpDate.hour, xmpDate.minute, xmpDate.second);

		} else if(xmpDate.hasDate) {

			propValue = DateTime(xmpDate.year, xmpDate.month, xmpDate.day);

		} 

	} catch(Exception ^) {

		propValue = Nullable<DateTime>();
	}

}

void MetaData::getProperty_Bool(String ^nameSpace, String ^propName, Nullable<bool> %propValue)
{
	bool xmpValue;

	bool result = xmpFile->getProperty_Bool(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName),
		xmpValue);

	if(result == true) {

		propValue = Nullable<bool>(xmpValue);

	} else {

		propValue = Nullable<bool>();
	}

}

void MetaData::getProperty_Float(String ^nameSpace, String ^propName, Nullable<double> %propValue)
{
	double xmpValue = 0;

	bool result = xmpFile->getProperty_Float(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName),
		xmpValue);

	if(result == true) {

		propValue = Nullable<double>(xmpValue);

	} else {

		propValue = Nullable<double>();
	}


}

void MetaData::getProperty_Int(String ^nameSpace, String ^propName, Nullable<long> %propValue)
{
	long xmpValue;

	bool result = xmpFile->getProperty_Int(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName),
		xmpValue);

	if(result == true) {

		propValue = Nullable<long>(xmpValue);

	} else {

		propValue = Nullable<long>();
	}


}	

void MetaData::getProperty_Int64(String ^nameSpace, String ^propName, Nullable<Int64> %propValue)
{
	Int64 xmpValue;

	bool result = xmpFile->getProperty_Int64(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName),
		xmpValue);

	if(result == true) {

		propValue = Nullable<Int64>(xmpValue);

	} else {

		propValue = Nullable<Int64>();
	}


}

void MetaData::setProperty_Date(String ^nameSpace, String ^propName, DateTime propValue) {

	XMP_DateTime xmpDate;
	xmpDate.year = propValue.Year;
	xmpDate.month = propValue.Month;
	xmpDate.day = propValue.Day;
	xmpDate.hour = propValue.Hour;
	xmpDate.minute = propValue.Minute;
	xmpDate.second = propValue.Second;
	xmpDate.hasDate = true; 
	xmpDate.hasTime = true;

	xmpFile->setProperty_Date(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName), 
		xmpDate);

}


void MetaData::setProperty(String ^nameSpace, String ^propName, String ^propValue, Consts::PropOptions options) {

	std::string propValueUTF8;
	WStringToUTF8(propValue, propValueUTF8);

	xmpFile->setProperty(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName), 
		propValueUTF8,
		(XMP_OptionBits)options);
}

bool MetaData::doesPropertyExists(String ^nameSpace, String ^propName) {

	bool exists = xmpFile->doesPropertyExists(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName));

	return(exists);
}

int MetaData::countArrayItems(String ^nameSpace, String ^arrayName)
{
	int count = xmpFile->countArrayItems(
		marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(arrayName));

	return(count);

}
void MetaData::getArrayItem(String ^nameSpace, String ^arrayName, int item, String^ %itemValue)
{
	std::string temp;

	bool result = xmpFile->getArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item, 
		temp);

	
	if(result == true) {
	
		UTF8ToWString(temp, itemValue);
	
	} else {

		itemValue = nullptr;
	}

	
}

bool MetaData::doesArrayItemExist(String ^nameSpace, String ^arrayName, int item) {

	bool result = xmpFile->doesArrayItemExist(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item);

	return(result);
}

void MetaData::setArrayItem(String ^nameSpace, String ^arrayName, int item, String ^itemValue, Consts::PropOptions options)
{
	std::string itemValueUTF8;
	WStringToUTF8(itemValue, itemValueUTF8);

	xmpFile->setArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item,
		itemValueUTF8,
		(XMP_OptionBits)options);

}


void MetaData::appendArrayItem(String ^nameSpace, String ^arrayName, Consts::PropOptions arrayOptions, String ^itemValue, Consts::PropOptions options)
{

	std::string itemValueUTF8;
	const char *temp = NULL;
	
	if(itemValue != nullptr) {
		
		WStringToUTF8(itemValue, itemValueUTF8);
		temp = itemValueUTF8.c_str();
	}

	xmpFile->appendArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		(XMP_OptionBits)arrayOptions, 
		temp,
		(XMP_OptionBits)options);
	
}

void MetaData::deleteArrayItem(String ^nameSpace, String ^arrayName, int item)
{
	xmpFile->deleteArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item);

}

void MetaData::getLocalizedText(String ^nameSpace, String ^textName, String ^genericLang,  String ^specificLang, String ^ %itemValue) 
{
	std::string temp;

	bool result = xmpFile->getLocalizedText(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(textName), 
		marshal_as<std::string>(genericLang), 
		marshal_as<std::string>(specificLang), 
		temp);

	if(result == true) {
	
		UTF8ToWString(temp, itemValue);
		
	} else {

		itemValue = nullptr;
	}

}

void MetaData::setLocalizedText(String ^nameSpace, String ^textName, String ^genericLang, String ^specificLang, String ^itemValue) 
{
	std::string itemValueUTF8;
	WStringToUTF8(itemValue, itemValueUTF8);

	xmpFile->setLocalizedText(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(textName), 
		marshal_as<std::string>(genericLang), 
		marshal_as<std::string>(specificLang), 
		itemValueUTF8);
}

void MetaData::iterate(Consts::IterOptions options, List<MetaDataProperty ^> ^%properties) {

	std::vector<XMPDLL::XMPProperty> propsTemp;

	xmpFile->iterate((XMP_OptionBits) options, propsTemp);

	properties->Clear();

	for(int i = 0; i < (int)propsTemp.size(); i++) {

		MetaDataProperty ^p = gcnew MetaDataProperty();
		p->nameSpace = marshal_as<String ^>(propsTemp[i].schemaNS);
		p->path = marshal_as<String ^>(propsTemp[i].propPath);

		String ^propVal;
		UTF8ToWString(propsTemp[i].propVal, propVal);

		p->value = propVal;

		properties->Add(p);

	}
}

void MetaData::iterate(String ^nameSpace, Consts::IterOptions options, List<MetaDataProperty ^> ^%properties) {

	std::vector<XMPDLL::XMPProperty> propsTemp;

	xmpFile->iterate(marshal_as<std::string>(nameSpace), (XMP_OptionBits) options, propsTemp);

	properties->Clear();

	for(int i = 0; i < (int)propsTemp.size(); i++) {

		MetaDataProperty ^p = gcnew MetaDataProperty();
		p->nameSpace = marshal_as<String ^>(propsTemp[i].schemaNS);
		p->path = marshal_as<String ^>(propsTemp[i].propPath);
		
		String ^propVal;
		UTF8ToWString(propsTemp[i].propVal, propVal);

		p->value = propVal;

		properties->Add(p);

	}
}

void MetaData::iterate(String ^nameSpace, List<MetaDataProperty ^> ^%properties) {

	std::vector<XMPDLL::XMPProperty> propsTemp;

	xmpFile->iterate(marshal_as<std::string>(nameSpace), propsTemp);

	properties->Clear();

	for(int i = 0; i < (int)propsTemp.size(); i++) {

		MetaDataProperty ^p = gcnew MetaDataProperty();
		p->nameSpace = marshal_as<String ^>(propsTemp[i].schemaNS);
		p->path = marshal_as<String ^>(propsTemp[i].propPath);
		
		String ^propVal;
		UTF8ToWString(propsTemp[i].propVal, propVal);

		p->value = propVal;

		properties->Add(p);

	}
}

bool MetaData::getStructField(String ^nameSpace, String ^structName, String ^fieldNameSpace, 
					String ^fieldName, String ^%fieldValue) 
{
	std::string temp;

	bool result = xmpFile->getStructField(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(structName), 
		marshal_as<std::string>(fieldNameSpace), 
		marshal_as<std::string>(fieldName), 
		temp);

	UTF8ToWString(temp, fieldValue);

	return(result);

}

void MetaData::setStructField(String ^nameSpace, String ^structName, String ^fieldNameSpace, 
					String ^fieldName, String ^fieldValue, XMP_OptionBits options) 
{

	std::string fieldValueUTF8;

	WStringToUTF8(fieldValue, fieldValueUTF8);

	xmpFile->setStructField(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(structName), 
		marshal_as<std::string>(fieldNameSpace), 
		marshal_as<std::string>(fieldName), 
		fieldValueUTF8,
		options);

}


DateTime MetaData::convertToDate(String ^dateString) {

	XMP_DateTime xmpDate;
	DateTime date;

	if(String::IsNullOrEmpty(dateString)) {
		return(DateTime::MinValue);
	}

	XMPDLL::convertToDate(marshal_as<std::string>(dateString), xmpDate);
		
	if(xmpDate.hasDate && xmpDate.hasTime) {

		date = DateTime(xmpDate.year, xmpDate.month, xmpDate.day, xmpDate.hour, xmpDate.minute, xmpDate.second);

	} else if(xmpDate.hasDate) {

		date = DateTime(xmpDate.year, xmpDate.month, xmpDate.day);

	} else {

		date = DateTime(DateTime::MinValue);
	}

	return(date);
}

void MetaData::composeArrayItemPath(String ^nameSpace, 
	String ^arrayName, 
	int itemIndex, 
	String ^%fullPath)
{
	std::string temp;

	XMPDLL::composeArrayItemPath(marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName),
		itemIndex,
		temp);

	fullPath = marshal_as<String ^>(temp);

}

void MetaData::composeStructFieldPath(String ^nameSpace, 
	String ^structName, 
	String ^fieldNameSpace, 
	String ^fieldName, 
	String ^%fullPath)
{

	std::string temp;

	XMPDLL::composeStructFieldPath(marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(structName),
		marshal_as<std::string>(fieldNameSpace),
		marshal_as<std::string>(fieldName),
		temp);

	fullPath = marshal_as<String ^>(temp);
}

void MetaData::encodeToBase64(String ^rawStr,
	String ^%encodedStr)
{

	std::string temp;

	XMPDLL::encodeToBase64(marshal_as<std::string>(rawStr), temp);

	encodedStr = marshal_as<String ^>(temp);
}

void MetaData::decodeFromBase64(String ^encodedStr,
	String ^%rawStr)
{
	std::string temp;

	XMPDLL::decodeFromBase64(marshal_as<std::string>(encodedStr), temp);

	rawStr = marshal_as<String ^>(temp);
}

void MetaData::getVersionInfo(VersionInfo ^%info) {

	XMP_VersionInfo versionInfo;

	XMPDLL::getVersionInfo(&versionInfo);

	info->build = versionInfo.build;

	info->flags = versionInfo.flags;
	info->isDebug = versionInfo.isDebug == 0 ? false : true;
	info->message =  marshal_as<String ^>(versionInfo.message);
	info->major = versionInfo.major;
	info->micro = versionInfo.micro;
	info->minor = versionInfo.minor;
}

// Convert wide String ^ to utf8 encoded std::string
void MetaData::WStringToUTF8(String ^input, std::string &output) {

	// Convert location to UTF8 string *
	array<Byte>^ encodedBytes = System::Text::Encoding::UTF8->GetBytes(input);

	// prevent GC moving the bytes around while this variable is on the stack
	pin_ptr<Byte> pinnedBytes = &encodedBytes[0];

	// Call the function, typecast from byte* -> char* is required
	char *utf8 = reinterpret_cast<char*>(pinnedBytes);

	output.assign(utf8);
}

// Convert utf8 encoded std::string to wide String ^
void MetaData::UTF8ToWString(const std::string &input, String ^%output) {
		
	// How long will the UTF-16 string be
	int wstrlen = MultiByteToWideChar(CP_UTF8, 0, input.c_str(), input.length(), NULL, NULL );
	
	// allocate a buffer
	wchar_t *buf = (wchar_t * ) malloc( wstrlen * 2 + 2 );
	// convert to UTF-16
	MultiByteToWideChar(CP_UTF8, 0, input.c_str(), input.length(), buf, wstrlen);
	// null terminate
	buf[wstrlen] = '\0';
	
	output = marshal_as<String ^>(buf);

	delete buf;
}

/*
MetaDataTreeNode ^MetaData::parse() {

	List<MetaDataProperty ^> ^propsList = gcnew List<MetaDataProperty ^>();
	
	iterate(kXMP_IterJustLeafNodes, propsList);

	MetaDataTreeNode ^root = gcnew MetaDataTreeNameSpaceNode("root");

	for each(MetaDataProperty ^p in propsList) {

		String ^path = p->path;

		//Debug::Print(p->path);
		root->insertNode(p->path, p->value);

	}

	return(root);
}
*/


}

