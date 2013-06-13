#pragma once

#include "MetaDataTree.h"

namespace imageviewer {

using namespace System;
using namespace System::Collections::Generic;


using namespace Aga::Controls::Tree;
using namespace System::IO;
using namespace System::Drawing;
using namespace System::ComponentModel;
using namespace System::Threading;

public ref class MetaDataItem 
{

protected:

	String ^name;
	String ^value;
	MetaDataTreeNode ^node;

public:

	MetaDataItem()
	{
		name = "";
		value = "";
		node = nullptr;
	}

	property String ^Name {

		String ^get() {

			return(name);
		}

		void set(String ^name) {

			this->name = name;
		}
	}

	property String ^Value {

		String ^get() {

			return(value);
		}

		void set(String ^value) {

			this->value = value;
		}
	}

	property MetaDataTreeNode ^Node {

		MetaDataTreeNode ^get() {

			return(node);
		}

		void set(MetaDataTreeNode ^node) {

			this->node = node;
		}
	}
};


}