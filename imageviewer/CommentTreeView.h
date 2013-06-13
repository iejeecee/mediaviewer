#pragma once

#include "CommentTreeNode.h"

using namespace System;
using namespace System::Drawing;
using namespace System::Windows::Forms;

namespace imageviewer {


	public ref class CommentTreeView : public TreeView
	{

	private:
		System::Drawing::Font^ font;

		System::Void treeView_DrawNode(System::Object ^sender, DrawTreeNodeEventArgs ^e) {

			CommentTreeNode ^node = dynamic_cast<CommentTreeNode ^>(e->Node);

			// Draw the background of the selected node. The NodeBounds 
			// method makes the highlight rectangle large enough to 
			// include the text of a node tag, if one is present.
			e->Graphics->FillRectangle( Brushes::Green, getNodeBounds( node ) );

			// Draw the node text.
			e->Graphics->DrawString( node->comment, font, Brushes::White, getNodeBounds( node ));//Rectangle::Inflate( e->Bounds, 2, 0 ) );
		}

		Rectangle getNodeBounds(CommentTreeNode ^node )
		{
			// Set the return value to the normal node bounds.
			Rectangle bounds = node->Bounds;

			if(String::IsNullOrEmpty(node->comment) == false)
			{
				// Retrieve a Graphics object from the TreeView handle 
				// and use it to calculate the display width of the tag.
				Graphics^ g = CreateGraphics();
				SizeF stringSize = g->MeasureString( node->comment, font);
				int width = (int)stringSize.Width + 6;
				int height = (int)(stringSize.Height + 6);

				// Adjust the node bounds using the calculated value.
				bounds.Offset( width / 2, height / 2 );
				bounds = Rectangle::Inflate( bounds, width / 2, height / 2 );
				g->~Graphics();
			}

			return bounds;
		}

	public:

		CommentTreeView() {

			font = gcnew System::Drawing::Font( "Helvetica",8,FontStyle::Bold );

			DrawMode = TreeViewDrawMode::OwnerDrawText;
			DrawNode += gcnew DrawTreeNodeEventHandler( this, &CommentTreeView::treeView_DrawNode );

			CommentTreeNode ^a = gcnew CommentTreeNode();
			a->author = "Bolle Billy";
			a->comment = "Bolle Billy\r\nsuck my dick";
			CommentTreeNode ^b = gcnew CommentTreeNode();
			b->author = "Hank Fatman";
			b->comment = "Hank Will.i.am.s\r\nI'm fbi boi";
			CommentTreeNode ^c = gcnew CommentTreeNode();
			c->author = "Trash Williams";
			c->comment = "Rich Bitch\r\nI will bloody your nose bitch";

			a->Nodes->Add(b);

			Nodes->Add(a);
			Nodes->Add(c);

		}


	};
}