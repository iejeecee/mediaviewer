#pragma once

namespace imageviewer {

using namespace System;

public ref class MetaDataProperty 
{
public:

	String ^nameSpace;
	String ^path;
	String ^value;

	MetaDataProperty() {

		nameSpace = "";
		path = "";
		value = "";
	}

	MetaDataProperty(const MetaDataProperty ^p) {

		if(p == nullptr) return;

		nameSpace = p->nameSpace;
		path = p->path;
		value = p->value;
	}

	virtual bool Equals(Object ^obj) override {

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

};

}