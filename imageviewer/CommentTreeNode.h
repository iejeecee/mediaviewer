#pragma once

using namespace System;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;

namespace imageviewer {


	public ref class CommentTreeNode : public TreeNode
	{

	private:

		String ^_author;
		String ^_comment;

	public:

		CommentTreeNode() {

		}

		property String ^comment {

			String ^get() {

				return(_comment);
			}

			void set(String ^comment) {

				_comment = comment;
			}
		}

		property String ^author {

			String ^get() {

				return(_author);
			}

			void set(String ^author) {

				_author = author;
			}
		}

	};
}