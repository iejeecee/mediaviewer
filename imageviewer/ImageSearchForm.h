#pragma once

#include "HttpRequest.h"
#include "MediaFormatConvert.h"
#include "ImageGridControl.h"
#include "ImageGridToolStripMenuItem.h"
#include "ImageGridPagerControl.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace Google::API::Search;


namespace imageviewer {

	/// <summary>
	/// Summary for ImageSearchForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class ImageSearchForm : public System::Windows::Forms::Form
	{

	private: GimageSearchClient ^imageSearch;
	private: int currentPage;
	private: String ^searchString;
	private: delegate void ShowResultsDelegate(List<ImageGridItem ^> ^imageData);

	private: System::Windows::Forms::Button^  button1;

	private: System::Windows::Forms::ComboBox^  comboBox1;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::ComboBox^  comboBox2;
	private: System::Windows::Forms::Label^  label3;
	private: System::Windows::Forms::ComboBox^  comboBox3;
	private: System::Windows::Forms::Label^  label4;
	private: System::Windows::Forms::ComboBox^  comboBox4;
	private: System::Windows::Forms::Label^  label5;
	private: System::Windows::Forms::ComboBox^  comboBox5;
	private: imageviewer::ImageGridControl^  imageGrid;
	private: imageviewer::ImageGridPagerControl^  pager;
	private: System::Windows::Forms::Button^  downloadButton;




	public: 
		delegate void viewImageEventHandler(System::Object^ sender, ImageGridMouseEventArgs ^e);
		event viewImageEventHandler ^OnViewImage;

		delegate void downloadEventHandler(System::Object^ sender, List<ImageGridItem ^> ^items);
		event downloadEventHandler ^OnDownload;

		delegate void ImageSearchMouseDoubleClickEventHandler(System::Object^ sender, ImageGridMouseEventArgs ^e);
		event ImageSearchMouseDoubleClickEventHandler ^OnImageSearchMouseDoubleClick;

		ImageSearchForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			imageSearch = gcnew GimageSearchClient(L"http://ijc7.blogspot.nl/");

			imageGrid->setNrImagePanels(16, false);
			imageGrid->ImageGridMouseDown += 
				gcnew EventHandler<ImageGridMouseEventArgs ^>(this, &ImageSearchForm::searchImage_Click);

			imageGrid->ImageGridMouseDoubleClick += 
				gcnew EventHandler<ImageGridMouseEventArgs ^>(this, &ImageSearchForm::searchImage_DoubleClick);

			imageGrid->UpdateImages += gcnew EventHandler<EventArgs ^>(this, &ImageSearchForm::updateImages_Event);

			pager->ImageGrid = imageGrid;

			currentPage = 0;

			comboBox1->SelectedIndex = 0;
			comboBox2->SelectedIndex = 0;
			comboBox3->SelectedIndex = 0;
			comboBox4->SelectedIndex = 0;
			comboBox5->SelectedIndex = 0;

		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~ImageSearchForm()
		{
			if (components)
			{
				delete components;
			}
		}

	protected: 
	private: System::Windows::Forms::TextBox^  textBox1;

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(ImageSearchForm::typeid));
			this->textBox1 = (gcnew System::Windows::Forms::TextBox());
			this->button1 = (gcnew System::Windows::Forms::Button());
			this->comboBox1 = (gcnew System::Windows::Forms::ComboBox());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->comboBox2 = (gcnew System::Windows::Forms::ComboBox());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->comboBox3 = (gcnew System::Windows::Forms::ComboBox());
			this->label4 = (gcnew System::Windows::Forms::Label());
			this->comboBox4 = (gcnew System::Windows::Forms::ComboBox());
			this->label5 = (gcnew System::Windows::Forms::Label());
			this->comboBox5 = (gcnew System::Windows::Forms::ComboBox());
			this->downloadButton = (gcnew System::Windows::Forms::Button());
			this->imageGrid = (gcnew imageviewer::ImageGridControl());
			this->pager = (gcnew imageviewer::ImageGridPagerControl());
			this->SuspendLayout();
			// 
			// textBox1
			// 
			this->textBox1->Location = System::Drawing::Point(10, 651);
			this->textBox1->Name = L"textBox1";
			this->textBox1->Size = System::Drawing::Size(333, 26);
			this->textBox1->TabIndex = 1;
			this->textBox1->KeyDown += gcnew System::Windows::Forms::KeyEventHandler(this, &ImageSearchForm::textBox1_KeyDown);
			// 
			// button1
			// 
			this->button1->Location = System::Drawing::Point(349, 648);
			this->button1->Name = L"button1";
			this->button1->Size = System::Drawing::Size(72, 33);
			this->button1->TabIndex = 2;
			this->button1->Text = L"Search";
			this->button1->UseVisualStyleBackColor = true;
			this->button1->Click += gcnew System::EventHandler(this, &ImageSearchForm::button1_Click);
			// 
			// comboBox1
			// 
			this->comboBox1->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->comboBox1->FormattingEnabled = true;
			this->comboBox1->Items->AddRange(gcnew cli::array< System::Object^  >(3) {L"active", L"moderate", L"off"});
			this->comboBox1->Location = System::Drawing::Point(61, 690);
			this->comboBox1->Name = L"comboBox1";
			this->comboBox1->Size = System::Drawing::Size(106, 28);
			this->comboBox1->TabIndex = 5;
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(11, 693);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(44, 20);
			this->label1->TabIndex = 6;
			this->label1->Text = L"Filter";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(182, 693);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(40, 20);
			this->label2->TabIndex = 8;
			this->label2->Text = L"Size";
			// 
			// comboBox2
			// 
			this->comboBox2->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->comboBox2->FormattingEnabled = true;
			this->comboBox2->Items->AddRange(gcnew cli::array< System::Object^  >(8) {L"all", L"huge", L"icon", L"large", L"medium", 
				L"small", L"xlarge", L"xxlarge"});
			this->comboBox2->Location = System::Drawing::Point(228, 690);
			this->comboBox2->Name = L"comboBox2";
			this->comboBox2->Size = System::Drawing::Size(106, 28);
			this->comboBox2->TabIndex = 7;
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(362, 693);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(92, 20);
			this->label3->TabIndex = 10;
			this->label3->Text = L"Colorization";
			// 
			// comboBox3
			// 
			this->comboBox3->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->comboBox3->FormattingEnabled = true;
			this->comboBox3->Items->AddRange(gcnew cli::array< System::Object^  >(3) {L"all", L"color", L"gray"});
			this->comboBox3->Location = System::Drawing::Point(460, 690);
			this->comboBox3->Name = L"comboBox3";
			this->comboBox3->Size = System::Drawing::Size(106, 28);
			this->comboBox3->TabIndex = 9;
			// 
			// label4
			// 
			this->label4->AutoSize = true;
			this->label4->Location = System::Drawing::Point(766, 693);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(43, 20);
			this->label4->TabIndex = 12;
			this->label4->Text = L"Type";
			// 
			// comboBox4
			// 
			this->comboBox4->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->comboBox4->FormattingEnabled = true;
			this->comboBox4->Items->AddRange(gcnew cli::array< System::Object^  >(6) {L"all", L"clipart", L"face", L"news", L"lineart", 
				L"photo"});
			this->comboBox4->Location = System::Drawing::Point(815, 690);
			this->comboBox4->Name = L"comboBox4";
			this->comboBox4->Size = System::Drawing::Size(106, 28);
			this->comboBox4->TabIndex = 11;
			// 
			// label5
			// 
			this->label5->AutoSize = true;
			this->label5->Location = System::Drawing::Point(588, 693);
			this->label5->Name = L"label5";
			this->label5->Size = System::Drawing::Size(46, 20);
			this->label5->TabIndex = 14;
			this->label5->Text = L"Color";
			// 
			// comboBox5
			// 
			this->comboBox5->DropDownStyle = System::Windows::Forms::ComboBoxStyle::DropDownList;
			this->comboBox5->FormattingEnabled = true;
			this->comboBox5->Items->AddRange(gcnew cli::array< System::Object^  >(11) {L"all", L"black", L"blue", L"brown", L"gray", L"green", 
				L"orange", L"pink", L"purple", L"red", L"white"});
			this->comboBox5->Location = System::Drawing::Point(640, 690);
			this->comboBox5->Name = L"comboBox5";
			this->comboBox5->Size = System::Drawing::Size(106, 28);
			this->comboBox5->TabIndex = 13;
			// 
			// downloadButton
			// 
			this->downloadButton->Location = System::Drawing::Point(460, 648);
			this->downloadButton->Name = L"downloadButton";
			this->downloadButton->Size = System::Drawing::Size(91, 33);
			this->downloadButton->TabIndex = 16;
			this->downloadButton->Text = L"Download";
			this->downloadButton->UseVisualStyleBackColor = true;
			this->downloadButton->Click += gcnew System::EventHandler(this, &ImageSearchForm::downloadButton_Click);
			// 
			// imageGrid
			// 
			this->imageGrid->Location = System::Drawing::Point(12, 13);
			this->imageGrid->Name = L"imageGrid";
			this->imageGrid->Size = System::Drawing::Size(911, 629);
			this->imageGrid->TabIndex = 15;
			// 
			// pager
			// 
			this->pager->Location = System::Drawing::Point(645, 648);
			this->pager->Name = L"pager";
			this->pager->Size = System::Drawing::Size(275, 38);
			this->pager->TabIndex = 17;
			// 
			// ImageSearchForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(932, 725);
			this->Controls->Add(this->pager);
			this->Controls->Add(this->downloadButton);
			this->Controls->Add(this->imageGrid);
			this->Controls->Add(this->label5);
			this->Controls->Add(this->comboBox5);
			this->Controls->Add(this->label4);
			this->Controls->Add(this->comboBox4);
			this->Controls->Add(this->label3);
			this->Controls->Add(this->comboBox3);
			this->Controls->Add(this->label2);
			this->Controls->Add(this->comboBox2);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->comboBox1);
			this->Controls->Add(this->button1);
			this->Controls->Add(this->textBox1);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->Name = L"ImageSearchForm";
			this->Text = L"Google Image Search";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	private: System::Void textBox1_KeyDown(System::Object^  sender, System::Windows::Forms::KeyEventArgs^  e) {

				 if(e->KeyCode != System::Windows::Forms::Keys::Enter) return;

				 doSearch();

			 }
	private: System::Void button1_Click(System::Object^  sender, System::EventArgs^  e) {

				 doSearch();
			 }

	private: System::Void updateImages_Event(System::Object^  sender, System::EventArgs^  e) {

				 setTitle();
			 }
	private: System::Void button2_Click(System::Object^  sender, System::EventArgs^  e) {

				 imageGrid->displayPrevPage();

			 }
	private: System::Void button3_Click(System::Object^  sender, System::EventArgs^  e) {

				 imageGrid->displayNextPage();

			 }

	private: void doSearch() {

				 startSearchAsync(nullptr);
			 }
	private: ImageGridContextMenuStrip ^createContextMenu(IImageResult ^imageInfo) {

				 ImageGridContextMenuStrip ^contextMenu = gcnew ImageGridContextMenuStrip(imageGrid);

				 ImageGridToolStripMenuItem ^viewItem = gcnew ImageGridToolStripMenuItem();
				 viewItem->Name = L"view";
				 viewItem->Text = L"View";
				 viewItem->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageSearchForm::viewImageToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(viewItem);

				 ImageGridToolStripMenuItem ^searchSimilar = gcnew ImageGridToolStripMenuItem();
				 searchSimilar->Name = L"searchSimilar";
				 searchSimilar->Text = L"Find similar images on " + imageInfo->VisibleUrl;
				 searchSimilar->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageSearchForm::searchSimilarToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(searchSimilar);		

				 ImageGridToolStripMenuItem ^selectAll = gcnew ImageGridToolStripMenuItem();
				 selectAll->Name = L"selectAll";
				 selectAll->Text = L"Select All Images";
				 selectAll->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageSearchForm::selectAllToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(selectAll);	

				 ImageGridToolStripMenuItem ^deselectAll = gcnew ImageGridToolStripMenuItem();
				 deselectAll->Name = L"deselectAll";
				 deselectAll->Text = L"Deselect All Images";
				 deselectAll->OnGridMouseDown += gcnew ImageGridToolStripMenuItem::GridMouseDownEventHandler(this, &ImageSearchForm::deselectAllToolStripMenuItem_MouseDown);

				 contextMenu->Items->Add(deselectAll);	

				 return(contextMenu);
			 }

	private: void startSearchAsync(String ^searchSite) {

				 searchString = textBox1->Text;

				 String ^safeLevel = comboBox1->Text;
				 String ^imageSize = comboBox2->Text;
				 String ^colorization = comboBox3->Text;	
				 String ^imageType = comboBox4->Text;
				 String ^imageColor = comboBox5->Text;					 

				 System::AsyncCallback ^callback = gcnew System::AsyncCallback(this, &ImageSearchForm::endSearchAsync);

				 imageSearch->BeginSearch(searchString, 64, safeLevel, 
					 imageSize, colorization, imageColor, imageType, "", searchSite, callback, nullptr);


			 }

	private: void endSearchAsync(IAsyncResult ^result) {

				 try {

					 Generic::IList<IImageResult ^> ^searchResult = imageSearch->EndSearch(result);

					 List<ImageGridItem ^> ^imageData = gcnew List<ImageGridItem ^>();

					 for(int i = 0; i < searchResult->Count; i++) {

						 IImageResult ^imageInfo = searchResult[i];

						 ImageGridItem ^item = gcnew ImageGridItem(imageInfo->TbImage->Url);
						 item->Data = imageInfo;

						 Uri ^uri = gcnew Uri(imageInfo->Url);
						 String ^name = Path::GetFileName(uri->AbsolutePath);

						 item->Caption = name + "\n" + imageInfo->VisibleUrl + "\n" + Convert::ToString(imageInfo->Width) + " x " + Convert::ToString(imageInfo->Height) + " - " + imageInfo->Title;
						 item->ContextMenu = createContextMenu(imageInfo);

						 imageData->Add(item);
					 }

					 ShowResultsDelegate ^showResults = gcnew ShowResultsDelegate(this, &ImageSearchForm::showResults);

					 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
					 args[0] = imageData;
					 this->BeginInvoke(showResults, args);

				 } catch(Exception ^e) {

					 log->Error("Google Image Search error", e);
					 MessageBox::Show(e->Message);
				 }

			 }

	private: void showResults(List<ImageGridItem ^> ^imageData) {

				 imageGrid->initializeImageData(imageData);
			 }

	private: void setTitle() {

				 String ^page = "";

				 if(imageGrid->getNrPages() > 0) {

					 page = "(" + Convert::ToString(imageGrid->getCurrentPage() + 1) + "/" 
						 + Convert::ToString(imageGrid->getNrPages()) + ") : ";
				 }

				 this->Text = L"Google Image Search " + page + searchString;

			 }

	private: System::Void searchImage_Click(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 if(e->Button == System::Windows::Forms::MouseButtons::Left) {

					 imageGrid->toggleImageSelected(e->imageNr);
				 }


			 }
	private: System::Void viewImageToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 OnViewImage(sender, e);
			 }
	private: System::Void searchSimilarToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 IImageResult ^imageInfo = dynamic_cast<IImageResult ^>(e->item->Data);

				 startSearchAsync(imageInfo->VisibleUrl);
			 }
	private: System::Void selectAllToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 imageGrid->setSelectedForAllImages(true);
			 }
	private: System::Void deselectAllToolStripMenuItem_MouseDown(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 imageGrid->setSelectedForAllImages(false);
			 }

	private: System::Void searchImage_DoubleClick(System::Object^  sender, ImageGridMouseEventArgs^  e) {

				 OnImageSearchMouseDoubleClick(sender, e);
			 }

	private: System::Void downloadButton_Click(System::Object^  sender, System::EventArgs^  e) {

				 List<ImageGridItem ^> ^items = imageGrid->getSelectedImageData();

				 if(items->Count > 0) {

					 imageGrid->setSelectedForAllImages(false);

					 OnDownload(this, items);
				 }
			 }
	};

}
