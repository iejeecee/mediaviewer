#pragma once

#include "FormatMetaData.h"
#include "XMP_Const.h"

namespace imageviewer {

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Diagnostics;
using namespace XMPLib;


public ref class MetaDataTreeNode abstract {

public:

	enum class Type {
		NAMESPACE,		
		ARRAY,
		STRUCT,
		PROPERTY,
		LANGUAGE,
		VALUE
	};

protected:

	String ^data;
	Type type;

	MetaDataTreeNode ^parent;

	virtual MetaDataTreeNode ^getChild(String ^data) = 0;
	virtual String ^getPath() = 0;

	MetaDataTreeNode(String ^data, Type type) {

		this->data = data;
		this->type = type;

		parent = nullptr;
	}

public:

	virtual bool hasChild(MetaDataTreeNode ^node) {

		for each(MetaDataTreeNode ^c in Child) {

			if(c->Equals(node)) {

				return(true);
			}
		}

		return(false);
	}

	virtual void insertChild(MetaDataTreeNode ^node) = 0;

	virtual bool hasNode(String ^path) {

		MetaDataTreeNode ^node = getNode(path);

		return(node == nullptr ? false : true);
	}

	virtual MetaDataTreeNode ^getNode(String ^path) {

		String ^head = "";
		String ^tail = "";
	
		for(int i = 0; i < path->Length; i++) {

			if(path[i] == '[' || path[i] == ':' || path[i] == '/') {

				if(i != 0) {

					tail = path->Substring(i);
					break;			

				} else if(path[i] == '[') {

					head += path[i];
				}

			} else {

				head += path[i];					
			}

		}

		MetaDataTreeNode ^node = getChild(head);
	
		if(node == nullptr) {

			return(nullptr);

		} else {

			if(String::IsNullOrEmpty(tail)) {

				return(node);
			
			} else {

				return(node = node->getNode(tail));
			}
		}
	}

	void insertNode(String ^data, String ^value);

	property MetaDataTreeNode ^Parent {

		MetaDataTreeNode ^get() {

			return(parent);
		}

		void set(MetaDataTreeNode ^parent) {

			this->parent = parent;
		}
	}

	virtual void clear() = 0;

	virtual property int Count {

		int get() = 0;
	}

	virtual property System::Collections::Generic::ICollection<MetaDataTreeNode ^> ^Child {

		System::Collections::Generic::ICollection<MetaDataTreeNode ^> ^get() = 0;
	}

	property Type NodeType {

		Type get() {

			return(type);
		}
	}

	property String ^Data {

		String ^get() {

			return(data);
		}

		void set(String ^data) {

			this->data = data;
		}
	}

	property String ^Path {

		String ^get() {

			String ^path = getPath();

			MetaDataTreeNode ^p = this;

			while(p->parent != nullptr && p->parent->parent != nullptr) {

				path = p->parent->getPath() + path;

				p = p->parent;
			}

			return(path);
		}

	}

	virtual String ^ToString() override {

		String ^info = data;

		return(info);
	}

	virtual void print(int tabs) {

		String ^output = "";

		for(int i = 0; i < tabs; i++) {

			output += "\t";
		}

		output += ToString() + "\n";

		System::Diagnostics::Debug::Write(output);

		for each(MetaDataTreeNode ^node in Child) {

			node->print(tabs + 1);

		}
	}

	virtual bool Equals(System::Object ^obj) override {

		MetaDataTreeNode ^node = dynamic_cast<MetaDataTreeNode ^>(obj);

		if(node == nullptr) return(false);

		if(node->NodeType != NodeType) return(false);

		if(node->Data->Equals(Data)) return(true);

		return(false);
	}

	MetaDataTreeNode ^intersection(MetaDataTreeNode ^tree);
};

public ref class MetaDataTreeDictionaryNode abstract : public MetaDataTreeNode {

protected:

	Dictionary<String ^, MetaDataTreeNode ^> ^child;

	virtual MetaDataTreeNode ^getChild(String ^data) override {

		MetaDataTreeNode ^node;
		bool exists = child->TryGetValue(data, node);

		if(exists == false) {

			return(nullptr);

		} else {

			return(node);
		}

	}

	MetaDataTreeDictionaryNode(String ^data, MetaDataTreeNode::Type type) : MetaDataTreeNode(data, type) {

		child = gcnew Dictionary<String ^, MetaDataTreeNode ^>();
	}

public:

	virtual void clear() override {

		child->Clear();

	}

	virtual property int Count {

		int get() override {

			return(child->Count);
		}
	}

	virtual property System::Collections::Generic::ICollection<MetaDataTreeNode ^> ^Child {

		System::Collections::Generic::ICollection<MetaDataTreeNode ^> ^get() override {

			return(child->Values);
		}
	}

	property MetaDataTreeNode ^default[String ^] {

	  MetaDataTreeNode ^get(String ^key) {
         
		  return(child[key]);
      }
      
	  void set(String ^key, MetaDataTreeNode ^node) {

		  child[key] = node;
			
      }
	}

	virtual bool hasChild(MetaDataTreeNode ^node) override {

		MetaDataTreeNode ^found = child[node->Data];

		if(found == nullptr) return(false);
		else return(found->Equals(node));
	}

	virtual void insertChild(MetaDataTreeNode ^node) override {

		child[node->Data] = node;
	
	}



};


public ref class MetaDataTreeArray : public MetaDataTreeNode {

protected:

	List<MetaDataTreeNode ^> ^child;

	int nameToIndex(String ^input) {

		if(String::IsNullOrEmpty(input)) return(-1);

		String ^temp = input->Substring(1, input->Length - 2);
		int index = Convert::ToInt32(temp) - 1;

		return(index);
	}

	String ^indexToName(int index) {

		String ^temp = Convert::ToString(index + 1);
		String ^output = "[" + temp + "]";

		return(output);
	}

	virtual MetaDataTreeNode ^getChild(String ^data) override {

		int index = nameToIndex(data);

		if(index >= Count) {

			return(nullptr);
		
		} else {
			
			return(child[index]);
		}
	}

	MetaDataTreeArray(String ^data, MetaDataTreeNode::Type type) : MetaDataTreeNode(data, type) {

		child = gcnew List<MetaDataTreeNode ^>();
	}

	virtual String ^getPath() override {

		return(data + "[]");
	}

public:

	MetaDataTreeArray(String ^data) : MetaDataTreeNode(data, MetaDataTreeNode::Type::ARRAY) {

		child = gcnew List<MetaDataTreeNode ^>();
	}

	virtual void clear() override {

		child->Clear();

	}

	virtual property int Count {

		int get() override {

			return(child->Count);
		}
	}

	virtual property System::Collections::Generic::ICollection<MetaDataTreeNode ^> ^Child {

		System::Collections::Generic::ICollection<MetaDataTreeNode ^> ^get() override {

			return(child);
		}
	}

	property MetaDataTreeNode ^default[int] {

	  MetaDataTreeNode ^get(int index) {
         
		  Debug::Assert(index < Count);

		  return(child[index]);
      }
      
	  void set(int index, MetaDataTreeNode ^node) {

		  Debug::Assert(index < Count);

		  child[index] = node;
			
      }
	}

	virtual bool hasChild(MetaDataTreeNode ^node) override {

		for each(MetaDataTreeNode ^c in child) {

			return(c->Equals(node));
		}

		return(false);
	}

	virtual void insertChild(MetaDataTreeNode ^node) override {

		child->Add(node);

	}

	virtual int getChildIndex(MetaDataTreeNode ^node) {

		for(int i = 0; i < Count; i++) {

			if(node == child[i]) return(i);
		}

		return(-1);
	}

	virtual void insertChild(String ^indexStr, MetaDataTreeNode ^node) {

		int index = nameToIndex(indexStr);

		if(index < Count) {

			child[index] = node;

		} else if(index == Count) {

			child->Add(node);

		} else if(index > Count) {

			Debug::Assert(false);
		}
	
	}

	virtual String ^ToString() override {

		String ^info = data;

		return(info);
	}
	
};

public ref class MetaDataTreeLanguage : public MetaDataTreeArray {


protected:

	virtual String ^getPath() override {

		return("/?xml:lang");
	}

public:

	MetaDataTreeLanguage(String ^data) : MetaDataTreeArray(data, MetaDataTreeNode::Type::LANGUAGE) {
		
	}

	virtual String ^ToString() override {

		String ^info = data;

		return(info);
	}
};

public ref class MetaDataTreeValue : public MetaDataTreeArray {

protected:

	virtual String ^getPath() override {

		return("");
	}

public:

	MetaDataTreeValue(String ^data) : MetaDataTreeArray(data, MetaDataTreeNode::Type::VALUE) {
		
	}

	virtual String ^ToString() override {

		String ^info = data;

		return(info);
	}
};

public ref class MetaDataTreeProperty : public MetaDataTreeArray {

protected:

	virtual String ^getPath() override {

		return(data);
	}

public:

	MetaDataTreeProperty(String ^data) : MetaDataTreeArray(data, MetaDataTreeNode::Type::PROPERTY) {

	}

	virtual String ^ToString() override {

		String ^info = data;

		return(info);
	}

	property String ^Value {

	  String ^get() {
         
		  if(Count == 0) return(nullptr);
		  else return(child[0]->Data);
		
      }
      
	  void set(String ^value) {

		  Debug::Assert(Count > 0);

		  child[0]->Data = value;
			
      }
	}
};

public ref class MetaDataTreeNameSpaceNode : public MetaDataTreeDictionaryNode {

protected:

	virtual String ^getPath() override {

		return(data + ":");
	}

public:

	MetaDataTreeNameSpaceNode(String ^data) : MetaDataTreeDictionaryNode(data, MetaDataTreeNode::Type::NAMESPACE) {

	}

	virtual String ^ToString() override {

		String ^info = data;

		return(info);
	}
};

public ref class MetaDataTreeStructNode : public MetaDataTreeDictionaryNode {

protected:

	virtual String ^getPath() override {

		return(data + "/");
	}

public:

	MetaDataTreeStructNode(String ^data) : MetaDataTreeDictionaryNode(data, MetaDataTreeNode::Type::STRUCT) {

	}

	virtual String ^ToString() override {

		String ^info = data;

		return(info);
	}
};

public ref class MetaDataTreeNodeFactory {

public:

	static MetaDataTreeNode ^create(String ^data, MetaDataTreeNode::Type type) {

		switch(type) {

			case MetaDataTreeNode::Type::ARRAY:
				{
					return(gcnew MetaDataTreeArray(data));
					break;
				}
			case MetaDataTreeNode::Type::NAMESPACE:
				{

					return(gcnew MetaDataTreeNameSpaceNode(data));
					break;
				}
			case MetaDataTreeNode::Type::STRUCT:
				{

					return(gcnew MetaDataTreeStructNode(data));
					break;
				}
			case MetaDataTreeNode::Type::PROPERTY:
				{

					return(gcnew MetaDataTreeProperty(data));
					break;
				}
			case MetaDataTreeNode::Type::VALUE:
				{

					return(gcnew MetaDataTreeValue(data));
					break;
				}
			default:
				{

					Debug::Assert(false);
					break;
				}
		}

		return(nullptr);
	}

	static MetaDataTreeNode ^copy(MetaDataTreeNode ^node) {

		switch(node->NodeType) {

			case MetaDataTreeNode::Type::ARRAY:
				{
					return(gcnew MetaDataTreeArray(node->Data));
					break;
				}
			case MetaDataTreeNode::Type::NAMESPACE:
				{

					return(gcnew MetaDataTreeNameSpaceNode(node->Data));
					break;
				}
			case MetaDataTreeNode::Type::STRUCT:
				{

					return(gcnew MetaDataTreeStructNode(node->Data));
					break;
				}
			case MetaDataTreeNode::Type::PROPERTY:
				{

					return(gcnew MetaDataTreeProperty(node->Data));
					break;
				}
			case MetaDataTreeNode::Type::VALUE:
				{

					return(gcnew MetaDataTreeValue(node->Data));
					break;
				}
			case MetaDataTreeNode::Type::LANGUAGE:
				{

					return(gcnew MetaDataTreeLanguage(node->Data));
					break;
				}
			default:
				{

					Debug::Assert(false);
					break;
				}
		}

		return(nullptr);
	}
};

public ref class MetaDataTree {

public:
	static MetaDataTreeNode ^create(MetaData ^data) {

		List<MetaDataProperty ^> ^propsList = gcnew List<MetaDataProperty ^>();

		data->iterate(Consts::IterOptions::XMP_IterJustLeafNodes, propsList);

		MetaDataTreeNode ^root = gcnew MetaDataTreeNameSpaceNode("root");

		for each(MetaDataProperty ^p in propsList) {

			String ^path = p->path;

			//Debug::Print(p->path);
			root->insertNode(p->path, p->value);

		}

		return(root);
	}
};


/*
public ref class GpsCoordinate : public MetaDataProperty
{
private:

	int degrees;
	int minutes;
	int seconds;
	int secondsFraction;
	System::Char direction;

	double decimal;

public:

	GpsCoordinate(MetaDataProperty ^p) : MetaDataProperty(p) {

		degrees = 0;
		minutes = 0;
		seconds = 0;
		secondsFraction = 0;
		direction = '0';
		decimal = 0;

		String ^coord = p->value;

		int s1 = coord->IndexOf(",");
		int s2 = coord->LastIndexOf(",");
		int s3 = coord->IndexOf(".");

		int s4 = (s2 == -1 || s1 == s2) ? s3 : s2;

		degrees = Convert::ToInt32(coord->Substring(0, s1));
		minutes = Convert::ToInt32(coord->Substring(s1 + 1, s4 - s1 - 1));

		int fractLength = coord->Length - s4 - 2;
		int temp = Convert::ToInt32(coord->Substring(s4 + 1, fractLength));

		if(s2 == -1 || s1 == s2) {

			secondsFraction = temp;

			double d = Math::Pow(10, fractLength);

			seconds = int((secondsFraction / d) * 60);	

			decimal = degrees + ((minutes + secondsFraction / d) / 60);

		} else {

			seconds = temp;
			secondsFraction = 0;
		
			decimal = degrees + (minutes / 60.0) + (seconds / 3600.0);
		}

		direction = coord[coord->Length - 1];

		
	}

	property double Decimal {

		double get() {

			return(decimal);
		}

		void set(double decimal) {

			this->decimal = decimal;

			degrees = (int)Math::Truncate(decimal);

			double fract = (decimal - Math::Truncate(decimal)) * 60;
			
			minutes = (int)Math::Truncate(fract);

			secondsFraction = (int)(fract - Math::Truncate(fract));

			seconds = int((1.0 / secondsFraction) * 60);
		}
	}

};
*/

}