#pragma once

using namespace System::Windows::Forms;
using namespace System::Drawing;
using namespace System::Drawing::Drawing2D;

namespace imageviewer {

	public ref class CustomToolTip : public System::Windows::Forms::Control
	{

	private:

	protected:

		virtual void OnPaint(PaintEventArgs ^e) override {

			Graphics ^g = e->Graphics;

			// Draw Background
			g->FillRectangle(gcnew SolidBrush(this->BackColor), ClientRectangle);

			Control::OnPaint(e);

			g->DrawString(Text, this->DefaultFont, Brushes::Black, 2.5, 2.5);

			// Draw Border
			g->DrawRectangle(Pens::Black, ClientRectangle.X, ClientRectangle.Y,
				ClientRectangle.Width - 1, ClientRectangle.Height - 1);

		}

	public:

		property String ^Text {

			virtual void set(String ^text) override {

				Graphics ^g = this->CreateGraphics();

				SizeF dim = g->MeasureString(text, DefaultFont);
				Size = Drawing::Size((int)dim.Width + 5, (int)dim.Height + 5);
				Control::Text = text;
			}

		}

		CustomToolTip() {

			this->Size = Drawing::Size(100,200);
			this->Location = Point(100,100);
		}

	};

}