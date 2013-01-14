#include "StdAfx.h"
#include "MetaData.h"
#include <msclr\marshal_cppstd.h>

using namespace System;
using namespace System::IO;
using namespace System::Text::RegularExpressions;
using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

namespace imageviewer {


MetaData::MetaData() {

	xmpFile = XMPLib::newXMPFile();
}

MetaData::~MetaData() {

	if(xmpFile != nullptr) {

		xmpFile->release();
	}
}

bool MetaData::open(String ^filename, XMP_OptionBits options)
{
	bool result = xmpFile->open(marshal_as<std::string>(filename), options);

	return(result);
}

bool MetaData::dumpToDisk(String ^filename) {

	return(xmpFile->dumpToDisk(marshal_as<std::string>(filename)));
}

bool MetaData::canPutXMP() 
{
	return(xmpFile->canPutXMP());
}

void MetaData::putXMP()
{
	if(canPutXMP() == false) {
		
		throw gcnew Exception("Cannot write Meta Data");

	} else {

		xmpFile->putXMP();
	}
}

bool MetaData::getProperty(String ^nameSpace, String ^propName, String^ %propValue)
{
	std::string temp;

	bool result = xmpFile->getProperty(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName), 
		temp);

	propValue = marshal_as<String ^>(temp);

	return(result);
}

void MetaData::deleteProperty(String ^nameSpace, String ^propName) {

	xmpFile->deleteProperty(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName));
}

bool MetaData::getProperty_Date(String ^nameSpace, String ^propName, DateTime %propValue)
{
	XMP_DateTime xmpDate;

	bool result = xmpFile->getProperty_Date(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName),
		xmpDate);

	if(xmpDate.hasDate && xmpDate.hasTime) {

		propValue = DateTime(xmpDate.year, xmpDate.month, xmpDate.day, xmpDate.hour, xmpDate.minute, xmpDate.second);

	} else if(xmpDate.hasDate) {

		propValue = DateTime(xmpDate.year, xmpDate.month, xmpDate.day);

	} else {

		propValue = DateTime::MinValue;
	}

	return(result);
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


void MetaData::setProperty(String ^nameSpace, String ^propName, String ^propValue, XMP_OptionBits options) {

	xmpFile->setProperty(marshal_as<std::string>(nameSpace),
		marshal_as<std::string>(propName), 
		marshal_as<std::string>(propValue),
		options);
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
bool MetaData::getArrayItem(String ^nameSpace, String ^arrayName, int item, String^ %itemValue)
{
	std::string temp;

	bool result = xmpFile->getArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item, 
		temp);

	itemValue = marshal_as<String ^>(temp);

	return(result);
}

bool MetaData::doesArrayItemExist(String ^nameSpace, String ^arrayName, int item) {

	bool result = xmpFile->doesArrayItemExist(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item);

	return(result);
}

void MetaData::setArrayItem(String ^nameSpace, String ^arrayName, int item, String ^itemValue, XMP_OptionBits options)
{
	xmpFile->setArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item,
		marshal_as<std::string>(itemValue),
		options);

}


void MetaData::appendArrayItem(String ^nameSpace, String ^arrayName, XMP_OptionBits arrayOptions, String ^itemValue, XMP_OptionBits options)
{

	const char *temp = NULL;
	marshal_context context;
	
	if(itemValue != nullptr) {

		temp = context.marshal_as<const char*>(itemValue);
	}

	xmpFile->appendArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		arrayOptions, 
		temp,
		options);

	

}

void MetaData::deleteArrayItem(String ^nameSpace, String ^arrayName, int item)
{
	xmpFile->deleteArrayItem(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(arrayName), 
		item);

}

bool MetaData::getLocalizedText(String ^nameSpace, String ^textName, String ^genericLang,  String ^specificLang, String ^ %itemValue) 
{
	std::string temp;

	bool result = xmpFile->getLocalizedText(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(textName), 
		marshal_as<std::string>(genericLang), 
		marshal_as<std::string>(specificLang), 
		temp);

	itemValue = marshal_as<String ^>(temp);

	return(result);

}

void MetaData::setLocalizedText(String ^nameSpace, String ^textName, String ^genericLang, String ^specificLang, String ^itemValue) 
{

	xmpFile->setLocalizedText(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(textName), 
		marshal_as<std::string>(genericLang), 
		marshal_as<std::string>(specificLang), 
		marshal_as<std::string>(itemValue));
}

void MetaData::iterate(XMP_OptionBits options, List<MetaDataProperty ^> ^%properties) {

	std::vector<XMPLib::XMPProperty> propsTemp;

	xmpFile->iterate(options, propsTemp);

	properties->Clear();

	for(int i = 0; i < (int)propsTemp.size(); i++) {

		MetaDataProperty ^p = gcnew MetaDataProperty();
		p->nameSpace = marshal_as<String ^>(propsTemp[i].schemaNS);
		p->path = marshal_as<String ^>(propsTemp[i].propPath);
		p->value = marshal_as<String ^>(propsTemp[i].propVal);

		properties->Add(p);

	}
}

void MetaData::iterate(String ^nameSpace, XMP_OptionBits options, List<MetaDataProperty ^> ^%properties) {

	std::vector<XMPLib::XMPProperty> propsTemp;

	xmpFile->iterate(marshal_as<std::string>(nameSpace), options, propsTemp);

	properties->Clear();

	for(int i = 0; i < (int)propsTemp.size(); i++) {

		MetaDataProperty ^p = gcnew MetaDataProperty();
		p->nameSpace = marshal_as<String ^>(propsTemp[i].schemaNS);
		p->path = marshal_as<String ^>(propsTemp[i].propPath);
		p->value = marshal_as<String ^>(propsTemp[i].propVal);

		properties->Add(p);

	}
}

void MetaData::iterate(String ^nameSpace, List<MetaDataProperty ^> ^%properties) {

	std::vector<XMPLib::XMPProperty> propsTemp;

	xmpFile->iterate(marshal_as<std::string>(nameSpace), propsTemp);

	properties->Clear();

	for(int i = 0; i < (int)propsTemp.size(); i++) {

		MetaDataProperty ^p = gcnew MetaDataProperty();
		p->nameSpace = marshal_as<String ^>(propsTemp[i].schemaNS);
		p->path = marshal_as<String ^>(propsTemp[i].propPath);
		p->value = marshal_as<String ^>(propsTemp[i].propVal);

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

	fieldValue = marshal_as<String ^>(temp);

	return(result);

}

void MetaData::setStructField(String ^nameSpace, String ^structName, String ^fieldNameSpace, 
					String ^fieldName, String ^fieldValue, XMP_OptionBits options) 
{

	xmpFile->setStructField(
		marshal_as<std::string>(nameSpace), 
		marshal_as<std::string>(structName), 
		marshal_as<std::string>(fieldNameSpace), 
		marshal_as<std::string>(fieldName), 
		marshal_as<std::string>(fieldValue),
		options);

}


DateTime MetaData::convertToDate(String ^dateString) {

	XMP_DateTime xmpDate;
	DateTime date;

	if(String::IsNullOrEmpty(dateString)) {
		return(DateTime::MinValue);
	}

	XMPLib::convertToDate(marshal_as<std::string>(dateString), xmpDate);
		
	if(xmpDate.hasDate && xmpDate.hasTime) {

		date = DateTime(xmpDate.year, xmpDate.month, xmpDate.day, xmpDate.hour, xmpDate.minute, xmpDate.second);

	} else if(xmpDate.hasDate) {

		date = DateTime(xmpDate.year, xmpDate.month, xmpDate.day);

	} else {

		date = DateTime::MinValue;
	}

	return(date);
}

void MetaData::composeArrayItemPath(String ^nameSpace, 
	String ^arrayName, 
	int itemIndex, 
	String ^%fullPath)
{
	std::string temp;

	XMPLib::composeArrayItemPath(marshal_as<std::string>(nameSpace), 
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

	XMPLib::composeStructFieldPath(marshal_as<std::string>(nameSpace), 
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

	XMPLib::encodeToBase64(marshal_as<std::string>(rawStr), temp);

	encodedStr = marshal_as<String ^>(temp);
}

void MetaData::decodeFromBase64(String ^encodedStr,
	String ^%rawStr)
{
	std::string temp;

	XMPLib::decodeFromBase64(marshal_as<std::string>(encodedStr), temp);

	rawStr = marshal_as<String ^>(temp);
}

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



}