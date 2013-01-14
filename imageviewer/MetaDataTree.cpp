#include "StdAfx.h"
#include "MetaDataTree.h"

using namespace System;
using namespace System::IO;
using namespace System::Text::RegularExpressions;

namespace imageviewer {

	void MetaDataTreeNode::insertNode(String ^name, String ^value) {

		String ^head = "";
		String ^tail = "";

		MetaDataTreeNode::Type type = MetaDataTreeNode::Type::ARRAY;

		for(int i = 0; i < name->Length; i++) {

			if(name[i] == '[' || name[i] == ':' || name[i] == '/') {

				if(i != 0) {

					tail = name->Substring(i);

					if(name[i] == '[') {

						type = MetaDataTreeNode::Type::ARRAY;

					} else if(name[i] == ':') {

						type = MetaDataTreeNode::Type::NAMESPACE;

					} else if(name[i] == '/') {

						type = MetaDataTreeNode::Type::STRUCT;
					}
					break;								

				} else if(name[i] == '[') {

					head += name[i];
				}

			} else {

				head += name[i];					
			}

		}

		MetaDataTreeNode ^node = getChild(head);

		if(node == nullptr) {

			if(String::IsNullOrEmpty(tail)) {

				node = gcnew MetaDataTreeValue(value);

				if(this->NodeType == Type::ARRAY) {

					node->parent = this;
					insertChild(node);

				} else {

					MetaDataTreeProperty ^prop = gcnew MetaDataTreeProperty(head);
					prop->parent = this;
					node->parent = prop;
					prop->insertChild(node);

					insertChild(prop);
				}
				
			} else {

				if(this->NodeType == Type::ARRAY) {

					node = MetaDataTreeNodeFactory::create("", type);

				} else {

					node = MetaDataTreeNodeFactory::create(head, type);
				}
				node->parent = this;

				insertChild(node);
				node->insertNode(tail, value);
			}

		} else {

			if(String::IsNullOrEmpty(tail)) {

				if(node->type == Type::VALUE) {

					node->Data = value;
				
				} else {

					MetaDataTreeProperty ^prop = dynamic_cast<MetaDataTreeProperty ^>(node);
					prop->Value = value;

				}

			} else {

				if(node->type == type) {

					node->insertNode(tail, value);

				} else {

					MetaDataTreeLanguage ^lang = gcnew MetaDataTreeLanguage(value);
					MetaDataTreeArray ^arr = dynamic_cast<MetaDataTreeArray ^>(node);
					arr->insertChild("[1]", lang);
				}
			}
		}

	}

	MetaDataTreeNode ^MetaDataTreeNode::intersection(MetaDataTreeNode ^tree) {

		MetaDataTreeNode ^result = nullptr;

		if(this->Equals(tree)) {

			result = MetaDataTreeNodeFactory::copy(tree);

			for each(MetaDataTreeNode ^a in Child) {

				for each(MetaDataTreeNode ^b in tree->Child) {

					MetaDataTreeNode ^intersection = a->intersection(b);
					if(intersection != nullptr) {

						intersection->Parent = result;
						result->insertChild(intersection);
						break;
					}
				}
			}
		} 

		return(result);
	}
}
