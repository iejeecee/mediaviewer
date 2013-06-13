#pragma once

#include "MetaDataItem.h"
#include "FormatMetaData.h"

namespace imageviewer {

using namespace System;
using namespace System::Collections::Generic;


using namespace Aga::Controls::Tree;
using namespace System::IO;
using namespace System::Drawing;
using namespace System::ComponentModel;
using namespace System::Threading;

public ref class MetaDataModel : ITreeModel
{

protected:

	MetaDataTreeNode ^root;

public:

	MetaDataModel(MetaDataTreeNode ^root)
	{
		this->root = root;
	}


	virtual System::Collections::IEnumerable ^GetChildren(TreePath ^treePath) 
	{
		List<MetaDataItem ^> ^items = gcnew List<MetaDataItem ^>();
		if(treePath->IsEmpty())
		{		

			for each(MetaDataTreeNode ^n in root->Child) {

				MetaDataItem ^item = gcnew MetaDataItem();
				item->Name = n->ToString();
				item->Node = n;

				items->Add(item);
			}
						
		}
		else
		{
		
			MetaDataItem ^item = dynamic_cast<MetaDataItem ^>(treePath->LastNode);

			for each(MetaDataTreeNode ^n in item->Node->Child) {

				MetaDataItem ^item = gcnew MetaDataItem();
				item->Node = n;
				item->Name = n->ToString();

				if(n->NodeType == MetaDataTreeNode::Type::PROPERTY) {

					MetaDataTreeProperty ^prop = dynamic_cast<MetaDataTreeProperty ^>(n);

					if(String::IsNullOrEmpty(prop->Value)) continue;

					item->Name = FormatMetaData::formatPropertyName(prop->ToString());
					item->Value = FormatMetaData::formatPropertyValue(prop->Path, prop->Value);

				} else if(n->NodeType == MetaDataTreeNode::Type::VALUE) {

					if(n->Parent->NodeType == MetaDataTreeNode::Type::PROPERTY) continue;

					item->Name = "";
					item->Value = FormatMetaData::formatPropertyValue(n->Path, n->Data);

				} else if(n->NodeType == MetaDataTreeNode::Type::LANGUAGE) {

					item->Name = "Language";
					item->Value = n->ToString();
				}

				if(n->Parent != nullptr && n->Parent->NodeType == MetaDataTreeNode::Type::ARRAY) {

					 MetaDataTreeArray ^arr = dynamic_cast<MetaDataTreeArray ^>(n->Parent);

					 int i = arr->getChildIndex(n);

					 item->Name = "[" + Convert::ToString(i) + "] " + item->Name;
				 }

				items->Add(item);

			}

		}
		return items;
	}

	virtual bool IsLeaf(TreePath ^treePath) 
	{
	
		MetaDataItem ^item = dynamic_cast<MetaDataItem ^>(treePath->LastNode);
		if(item->Node->Count == 0) {

			return(true);

		} else {

			return(false);
		}
	}


	virtual event EventHandler<TreeModelEventArgs ^> ^NodesChanged;
	virtual event EventHandler<TreeModelEventArgs ^> ^NodesInserted;
	virtual event EventHandler<TreeModelEventArgs ^> ^NodesRemoved;
	virtual event EventHandler<TreePathEventArgs ^> ^StructureChanged;
};


}