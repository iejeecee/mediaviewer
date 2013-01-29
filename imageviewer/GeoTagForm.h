// http://notions.okuda.ca/2009/06/11/calling-javascript-in-a-webbrowser-control-from-c/
#pragma once
#include "GeoTagScriptInterface.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::IO;
using namespace System::Web::UI;
using namespace System::Text;


namespace imageviewer {

	/// <summary>
	/// Summary for GeoTagForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class GeoTagForm : public System::Windows::Forms::Form
	{
	public:
		GeoTagForm(List<String ^> ^fileNames)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			if(fileNames->Count == 1) {

				Text = fileNames[0] + " - Geo Tag";

			} else {

				Text = Convert::ToString(fileNames->Count) + " images - Geo Tag";
			}

			script = gcnew GeoTagScriptInterface(webBrowser, fileNames);
			script->Initialized += gcnew EventHandler<EventArgs ^>(this, &GeoTagForm::script_Initialized);
			script->PlaceMarkClicked += gcnew EventHandler<GeoTagFileData ^>(this, &GeoTagForm::script_PlaceMarkClicked);
			script->PlaceMarkMoved += gcnew EventHandler<GeoTagFileData ^>(this, &GeoTagForm::script_PlaceMarkMoved);
			script->EndPlaceMarkMoved += gcnew EventHandler<GeoTagFileData ^>(this, &GeoTagForm::script_EndPlaceMarkMoved);
			script->AddressUpdate += gcnew EventHandler<GEventArgs<String ^> ^>(this, &GeoTagForm::script_AddressUpdate);

			webBrowser->ObjectForScripting = script;

			String ^binDir = System::IO::Path::GetDirectoryName( 
				System::Reflection::Assembly::GetExecutingAssembly()->GetName()->CodeBase );
			binDir = binDir->Replace("file:/", "file:///");
			webBrowser->Url = gcnew Uri(binDir + "/geotag.htm");

			for each (GeoTagFileData ^data in script->GeoTagData) {

				imageListBox->Items->Add(data);
			}

			pictureBox->ImageLocation = script->GeoTagData[0]->FilePath;
			longitudeTextBox->Text = Convert::ToString(script->GeoTagData[0]->GeoTag->longitude->Decimal);
			latitudeTextBox->Text = Convert::ToString(script->GeoTagData[0]->GeoTag->latitude->Decimal);

		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~GeoTagForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::WebBrowser^  webBrowser;
	private: System::Windows::Forms::SplitContainer^  splitContainer1;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::ListBox^  imageListBox;



	private: System::Windows::Forms::TextBox^  longitudeTextBox;

	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::TextBox^  latitudeTextBox;
	private: System::Windows::Forms::PictureBox^  pictureBox;
	private: System::Windows::Forms::Button^  cancelButton;

	private: System::Windows::Forms::Button^  okButton;
	private: System::Windows::Forms::Button^  createButton;


	private: System::Windows::Forms::Button^  flyToButton;
	private: System::Windows::Forms::Label^  label4;
	private: System::Windows::Forms::Label^  label3;
	private: System::Windows::Forms::TextBox^  locationTextBox;
	private: System::Windows::Forms::TextBox^  adressTextBox;
	private: System::Windows::Forms::ToolTip^  toolTip1;
	private: System::Windows::Forms::Button^  deleteButton;
	private: System::Windows::Forms::TabControl^  tabControl1;
	private: System::Windows::Forms::TabPage^  tabPage1;
	private: System::Windows::Forms::TabPage^  tabPage2;
	private: System::Windows::Forms::CheckBox^  bordersCheckBox;
	private: System::Windows::Forms::CheckBox^  roadsCheckBox;
	private: System::Windows::Forms::CheckBox^  terrainCheckBox;
	private: System::Windows::Forms::CheckBox^  buildingsCheckBox;
	private: System::ComponentModel::IContainer^  components;



	protected: 

	protected: 

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>


#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->components = (gcnew System::ComponentModel::Container());
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(GeoTagForm::typeid));
			this->webBrowser = (gcnew System::Windows::Forms::WebBrowser());
			this->splitContainer1 = (gcnew System::Windows::Forms::SplitContainer());
			this->tabControl1 = (gcnew System::Windows::Forms::TabControl());
			this->tabPage1 = (gcnew System::Windows::Forms::TabPage());
			this->adressTextBox = (gcnew System::Windows::Forms::TextBox());
			this->pictureBox = (gcnew System::Windows::Forms::PictureBox());
			this->locationTextBox = (gcnew System::Windows::Forms::TextBox());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->imageListBox = (gcnew System::Windows::Forms::ListBox());
			this->latitudeTextBox = (gcnew System::Windows::Forms::TextBox());
			this->longitudeTextBox = (gcnew System::Windows::Forms::TextBox());
			this->createButton = (gcnew System::Windows::Forms::Button());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->label4 = (gcnew System::Windows::Forms::Label());
			this->flyToButton = (gcnew System::Windows::Forms::Button());
			this->deleteButton = (gcnew System::Windows::Forms::Button());
			this->tabPage2 = (gcnew System::Windows::Forms::TabPage());
			this->buildingsCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->terrainCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->bordersCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->roadsCheckBox = (gcnew System::Windows::Forms::CheckBox());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			this->okButton = (gcnew System::Windows::Forms::Button());
			this->toolTip1 = (gcnew System::Windows::Forms::ToolTip(this->components));
			this->splitContainer1->Panel1->SuspendLayout();
			this->splitContainer1->Panel2->SuspendLayout();
			this->splitContainer1->SuspendLayout();
			this->tabControl1->SuspendLayout();
			this->tabPage1->SuspendLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->pictureBox))->BeginInit();
			this->tabPage2->SuspendLayout();
			this->SuspendLayout();
			// 
			// webBrowser
			// 
			this->webBrowser->AllowWebBrowserDrop = false;
			this->webBrowser->Dock = System::Windows::Forms::DockStyle::Fill;
			this->webBrowser->IsWebBrowserContextMenuEnabled = false;
			this->webBrowser->Location = System::Drawing::Point(0, 0);
			this->webBrowser->MinimumSize = System::Drawing::Size(20, 20);
			this->webBrowser->Name = L"webBrowser";
			this->webBrowser->Size = System::Drawing::Size(698, 735);
			this->webBrowser->TabIndex = 0;
			this->webBrowser->TabStop = false;
			this->webBrowser->WebBrowserShortcutsEnabled = false;
			// 
			// splitContainer1
			// 
			this->splitContainer1->BorderStyle = System::Windows::Forms::BorderStyle::Fixed3D;
			this->splitContainer1->Dock = System::Windows::Forms::DockStyle::Fill;
			this->splitContainer1->IsSplitterFixed = true;
			this->splitContainer1->Location = System::Drawing::Point(0, 0);
			this->splitContainer1->Name = L"splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this->splitContainer1->Panel1->Controls->Add(this->tabControl1);
			this->splitContainer1->Panel1->Controls->Add(this->cancelButton);
			this->splitContainer1->Panel1->Controls->Add(this->okButton);
			// 
			// splitContainer1.Panel2
			// 
			this->splitContainer1->Panel2->Controls->Add(this->webBrowser);
			this->splitContainer1->Size = System::Drawing::Size(1014, 739);
			this->splitContainer1->SplitterDistance = 308;
			this->splitContainer1->TabIndex = 1;
			// 
			// tabControl1
			// 
			this->tabControl1->Controls->Add(this->tabPage1);
			this->tabControl1->Controls->Add(this->tabPage2);
			this->tabControl1->Location = System::Drawing::Point(-2, 0);
			this->tabControl1->Name = L"tabControl1";
			this->tabControl1->SelectedIndex = 0;
			this->tabControl1->Size = System::Drawing::Size(308, 678);
			this->tabControl1->TabIndex = 14;
			// 
			// tabPage1
			// 
			this->tabPage1->Controls->Add(this->adressTextBox);
			this->tabPage1->Controls->Add(this->pictureBox);
			this->tabPage1->Controls->Add(this->locationTextBox);
			this->tabPage1->Controls->Add(this->label1);
			this->tabPage1->Controls->Add(this->label2);
			this->tabPage1->Controls->Add(this->imageListBox);
			this->tabPage1->Controls->Add(this->latitudeTextBox);
			this->tabPage1->Controls->Add(this->longitudeTextBox);
			this->tabPage1->Controls->Add(this->createButton);
			this->tabPage1->Controls->Add(this->label3);
			this->tabPage1->Controls->Add(this->label4);
			this->tabPage1->Controls->Add(this->flyToButton);
			this->tabPage1->Controls->Add(this->deleteButton);
			this->tabPage1->Location = System::Drawing::Point(4, 29);
			this->tabPage1->Name = L"tabPage1";
			this->tabPage1->Padding = System::Windows::Forms::Padding(3);
			this->tabPage1->Size = System::Drawing::Size(300, 645);
			this->tabPage1->TabIndex = 0;
			this->tabPage1->Text = L"Geo Tag";
			this->tabPage1->UseVisualStyleBackColor = true;
			// 
			// adressTextBox
			// 
			this->adressTextBox->Location = System::Drawing::Point(10, 560);
			this->adressTextBox->Multiline = true;
			this->adressTextBox->Name = L"adressTextBox";
			this->adressTextBox->ReadOnly = true;
			this->adressTextBox->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->adressTextBox->Size = System::Drawing::Size(280, 79);
			this->adressTextBox->TabIndex = 9;
			// 
			// pictureBox
			// 
			this->pictureBox->Location = System::Drawing::Point(12, 372);
			this->pictureBox->Name = L"pictureBox";
			this->pictureBox->Size = System::Drawing::Size(276, 182);
			this->pictureBox->SizeMode = System::Windows::Forms::PictureBoxSizeMode::Zoom;
			this->pictureBox->TabIndex = 5;
			this->pictureBox->TabStop = false;
			// 
			// locationTextBox
			// 
			this->locationTextBox->Enabled = false;
			this->locationTextBox->Location = System::Drawing::Point(11, 30);
			this->locationTextBox->Name = L"locationTextBox";
			this->locationTextBox->Size = System::Drawing::Size(236, 26);
			this->locationTextBox->TabIndex = 5;
			this->locationTextBox->Enter += gcnew System::EventHandler(this, &GeoTagForm::locationTextBox_Enter);
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(10, 189);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(67, 20);
			this->label1->TabIndex = 12;
			this->label1->Text = L"Latitude";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(10, 251);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(80, 20);
			this->label2->TabIndex = 13;
			this->label2->Text = L"Longitude";
			// 
			// imageListBox
			// 
			this->imageListBox->Enabled = false;
			this->imageListBox->FormattingEnabled = true;
			this->imageListBox->ItemHeight = 20;
			this->imageListBox->Location = System::Drawing::Point(10, 90);
			this->imageListBox->Name = L"imageListBox";
			this->imageListBox->Size = System::Drawing::Size(280, 84);
			this->imageListBox->TabIndex = 6;
			this->imageListBox->SelectedIndexChanged += gcnew System::EventHandler(this, &GeoTagForm::imageListBox_SelectedIndexChanged);
			// 
			// latitudeTextBox
			// 
			this->latitudeTextBox->Location = System::Drawing::Point(10, 212);
			this->latitudeTextBox->Name = L"latitudeTextBox";
			this->latitudeTextBox->ReadOnly = true;
			this->latitudeTextBox->Size = System::Drawing::Size(280, 26);
			this->latitudeTextBox->TabIndex = 7;
			// 
			// longitudeTextBox
			// 
			this->longitudeTextBox->Location = System::Drawing::Point(10, 274);
			this->longitudeTextBox->Name = L"longitudeTextBox";
			this->longitudeTextBox->ReadOnly = true;
			this->longitudeTextBox->Size = System::Drawing::Size(280, 26);
			this->longitudeTextBox->TabIndex = 8;
			// 
			// createButton
			// 
			this->createButton->Enabled = false;
			this->createButton->Location = System::Drawing::Point(12, 316);
			this->createButton->Name = L"createButton";
			this->createButton->Size = System::Drawing::Size(92, 39);
			this->createButton->TabIndex = 1;
			this->createButton->Text = L"Create";
			this->toolTip1->SetToolTip(this->createButton, L"Create geo tag");
			this->createButton->UseVisualStyleBackColor = true;
			this->createButton->Click += gcnew System::EventHandler(this, &GeoTagForm::createButton_Click);
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(10, 3);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(47, 20);
			this->label3->TabIndex = 10;
			this->label3->Text = L"Fly to";
			// 
			// label4
			// 
			this->label4->AutoSize = true;
			this->label4->Location = System::Drawing::Point(10, 67);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(62, 20);
			this->label4->TabIndex = 11;
			this->label4->Text = L"Images";
			// 
			// flyToButton
			// 
			this->flyToButton->BackColor = System::Drawing::SystemColors::Control;
			this->flyToButton->Enabled = false;
			this->flyToButton->Image = (cli::safe_cast<System::Drawing::Image^  >(resources->GetObject(L"flyToButton.Image")));
			this->flyToButton->Location = System::Drawing::Point(253, 30);
			this->flyToButton->Name = L"flyToButton";
			this->flyToButton->Size = System::Drawing::Size(36, 26);
			this->flyToButton->TabIndex = 0;
			this->toolTip1->SetToolTip(this->flyToButton, L"Begin search");
			this->flyToButton->UseVisualStyleBackColor = false;
			this->flyToButton->Click += gcnew System::EventHandler(this, &GeoTagForm::flyToButton_Click);
			// 
			// deleteButton
			// 
			this->deleteButton->Enabled = false;
			this->deleteButton->Location = System::Drawing::Point(110, 316);
			this->deleteButton->Name = L"deleteButton";
			this->deleteButton->Size = System::Drawing::Size(92, 39);
			this->deleteButton->TabIndex = 2;
			this->deleteButton->Text = L"Delete";
			this->toolTip1->SetToolTip(this->deleteButton, L"Delete geo tag");
			this->deleteButton->UseVisualStyleBackColor = true;
			this->deleteButton->Click += gcnew System::EventHandler(this, &GeoTagForm::deleteButton_Click);
			// 
			// tabPage2
			// 
			this->tabPage2->Controls->Add(this->buildingsCheckBox);
			this->tabPage2->Controls->Add(this->terrainCheckBox);
			this->tabPage2->Controls->Add(this->bordersCheckBox);
			this->tabPage2->Controls->Add(this->roadsCheckBox);
			this->tabPage2->Location = System::Drawing::Point(4, 29);
			this->tabPage2->Name = L"tabPage2";
			this->tabPage2->Padding = System::Windows::Forms::Padding(3);
			this->tabPage2->Size = System::Drawing::Size(300, 645);
			this->tabPage2->TabIndex = 1;
			this->tabPage2->Text = L"Options";
			this->tabPage2->UseVisualStyleBackColor = true;
			// 
			// buildingsCheckBox
			// 
			this->buildingsCheckBox->AutoSize = true;
			this->buildingsCheckBox->Enabled = false;
			this->buildingsCheckBox->Location = System::Drawing::Point(11, 104);
			this->buildingsCheckBox->Name = L"buildingsCheckBox";
			this->buildingsCheckBox->Size = System::Drawing::Size(117, 24);
			this->buildingsCheckBox->TabIndex = 3;
			this->buildingsCheckBox->Text = L"3D Buildings";
			this->buildingsCheckBox->UseVisualStyleBackColor = true;
			this->buildingsCheckBox->CheckedChanged += gcnew System::EventHandler(this, &GeoTagForm::buildingsCheckBox_CheckedChanged);
			// 
			// terrainCheckBox
			// 
			this->terrainCheckBox->AutoSize = true;
			this->terrainCheckBox->Checked = true;
			this->terrainCheckBox->CheckState = System::Windows::Forms::CheckState::Checked;
			this->terrainCheckBox->Enabled = false;
			this->terrainCheckBox->Location = System::Drawing::Point(11, 74);
			this->terrainCheckBox->Name = L"terrainCheckBox";
			this->terrainCheckBox->Size = System::Drawing::Size(146, 24);
			this->terrainCheckBox->TabIndex = 2;
			this->terrainCheckBox->Text = L"Terrain Elevation";
			this->terrainCheckBox->UseVisualStyleBackColor = true;
			this->terrainCheckBox->CheckedChanged += gcnew System::EventHandler(this, &GeoTagForm::terrainCheckBox_CheckedChanged);
			// 
			// bordersCheckBox
			// 
			this->bordersCheckBox->AutoSize = true;
			this->bordersCheckBox->Checked = true;
			this->bordersCheckBox->CheckState = System::Windows::Forms::CheckState::Checked;
			this->bordersCheckBox->Enabled = false;
			this->bordersCheckBox->Location = System::Drawing::Point(11, 44);
			this->bordersCheckBox->Name = L"bordersCheckBox";
			this->bordersCheckBox->Size = System::Drawing::Size(84, 24);
			this->bordersCheckBox->TabIndex = 1;
			this->bordersCheckBox->Text = L"Borders";
			this->bordersCheckBox->UseVisualStyleBackColor = true;
			this->bordersCheckBox->CheckedChanged += gcnew System::EventHandler(this, &GeoTagForm::bordersCheckBox_CheckedChanged);
			// 
			// roadsCheckBox
			// 
			this->roadsCheckBox->AutoSize = true;
			this->roadsCheckBox->Checked = true;
			this->roadsCheckBox->CheckState = System::Windows::Forms::CheckState::Checked;
			this->roadsCheckBox->Enabled = false;
			this->roadsCheckBox->Location = System::Drawing::Point(11, 14);
			this->roadsCheckBox->Name = L"roadsCheckBox";
			this->roadsCheckBox->Size = System::Drawing::Size(75, 24);
			this->roadsCheckBox->TabIndex = 0;
			this->roadsCheckBox->Text = L"Roads";
			this->roadsCheckBox->UseVisualStyleBackColor = true;
			this->roadsCheckBox->CheckedChanged += gcnew System::EventHandler(this, &GeoTagForm::roadsCheckBox_CheckedChanged);
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(155, 684);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(92, 41);
			this->cancelButton->TabIndex = 4;
			this->cancelButton->Text = L"Close";
			this->toolTip1->SetToolTip(this->cancelButton, L"Cancel changes");
			this->cancelButton->UseVisualStyleBackColor = true;
			this->cancelButton->Click += gcnew System::EventHandler(this, &GeoTagForm::cancelButton_Click);
			// 
			// okButton
			// 
			this->okButton->Location = System::Drawing::Point(57, 684);
			this->okButton->Name = L"okButton";
			this->okButton->Size = System::Drawing::Size(92, 41);
			this->okButton->TabIndex = 3;
			this->okButton->Text = L"Save";
			this->toolTip1->SetToolTip(this->okButton, L"Save changes");
			this->okButton->UseVisualStyleBackColor = true;
			this->okButton->Click += gcnew System::EventHandler(this, &GeoTagForm::okButton_Click);
			// 
			// GeoTagForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(1014, 739);
			this->Controls->Add(this->splitContainer1);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->Name = L"GeoTagForm";
			this->Text = L"Geo Tag";
			this->FormClosed += gcnew System::Windows::Forms::FormClosedEventHandler(this, &GeoTagForm::geoTagForm_FormClosed);
			this->splitContainer1->Panel1->ResumeLayout(false);
			this->splitContainer1->Panel2->ResumeLayout(false);
			this->splitContainer1->ResumeLayout(false);
			this->tabControl1->ResumeLayout(false);
			this->tabPage1->ResumeLayout(false);
			this->tabPage1->PerformLayout();
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->pictureBox))->EndInit();
			this->tabPage2->ResumeLayout(false);
			this->tabPage2->PerformLayout();
			this->ResumeLayout(false);

		}
#pragma endregion

	private: GeoTagScriptInterface ^script;

	private: System::Void script_Initialized(System::Object^  sender, System::EventArgs^  e) {

				adressTextBox->Enabled = true;
				locationTextBox->Enabled = true;
				flyToButton->Enabled = true;
				this->ActiveControl = flyToButton;
				imageListBox->Enabled = true;
				imageListBox->SelectedIndex = 0;

				roadsCheckBox->Enabled = true;
				bordersCheckBox->Enabled = true;
				terrainCheckBox->Enabled = true;
				buildingsCheckBox->Enabled = true;
			 }

	private: System::Void imageListBox_SelectedIndexChanged(System::Object^  sender, System::EventArgs^  e) 
			 {

				 int i = imageListBox->SelectedIndex;
				 if(i == -1) return;

				 pictureBox->ImageLocation = script->GeoTagData[i]->FilePath;
				 longitudeTextBox->Text = Convert::ToString(script->GeoTagData[i]->GeoTag->longitude->Decimal);
				 latitudeTextBox->Text = Convert::ToString(script->GeoTagData[i]->GeoTag->latitude->Decimal);

				 script->lookAtPlaceMark(script->GeoTagData[i]);

				 script->reverseGeoCodePlaceMark(script->GeoTagData[i]);

				 if(script->GeoTagData[i]->HasGeoTag == false) {

					 createButton->Enabled = true;
					 deleteButton->Enabled = false;

				 } else {

					 createButton->Enabled = false;
					 deleteButton->Enabled = true;
				 }

			 }

	private: System::Void script_AddressUpdate(System::Object^  sender, GEventArgs<String ^> ^address) 
			 {

				 adressTextBox->Text = address->Value;

			 }

	private: System::Void script_PlaceMarkClicked(System::Object^  sender, GeoTagFileData ^item) 
			 {

				 imageListBox->SelectedItem = item;

			 }

	private: System::Void script_PlaceMarkMoved(System::Object^  sender, GeoTagFileData ^item) 
			 {

				 longitudeTextBox->Text = Convert::ToString(item->GeoTag->longitude->Decimal);
				 latitudeTextBox->Text = Convert::ToString(item->GeoTag->latitude->Decimal);

			 }

	private: System::Void script_EndPlaceMarkMoved(System::Object^  sender, GeoTagFileData ^item) 
			 {

				script->reverseGeoCodePlaceMark(item);

			 }

	private: System::Void geoTagForm_FormClosed(System::Object^  sender, System::Windows::Forms::FormClosedEventArgs^  e) 
			 {

				 delete webBrowser;
			 }

	private: System::Void flyToButton_Click(System::Object^  sender, System::EventArgs^  e) 
			 {

				 if(!String::IsNullOrEmpty(locationTextBox->Text)) {

					 script->flyTo(locationTextBox->Text);
				 }
			 }

	private: System::Void createButton_Click(System::Object^  sender, System::EventArgs^  e) 
			 {
				 int i = imageListBox->SelectedIndex;
				 if(i == -1) return;

				 script->createPlaceMark(script->GeoTagData[i], true);

				 imageListBox->SelectedIndex = -1;
				 imageListBox->SelectedIndex = i;
			 }

private: System::Void deleteButton_Click(System::Object^  sender, System::EventArgs^  e) {
				 
				 int i = imageListBox->SelectedIndex;
				 if(i == -1) return;

				 script->deletePlaceMark(script->GeoTagData[i]);

				 imageListBox->SelectedIndex = -1;
				 imageListBox->SelectedIndex = i;

		 }
private: System::Void okButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 try {

				 for each(GeoTagFileData ^data in script->GeoTagData) {

					 data->saveToDisk();
				 }

				  MessageBox::Show("Done saving data", "Success");

			 } catch (Exception ^e) {

				 MessageBox::Show(e->Message, "Error saving Geo Tag");
			 } 
		 }
private: System::Void cancelButton_Click(System::Object^  sender, System::EventArgs^  e) {

			  Form::Close();
		 }
private: System::Void locationTextBox_Enter(System::Object^  sender, System::EventArgs^  e) {
			
			
		 }
private: System::Void roadsCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 script->setRoads(roadsCheckBox->Checked);
		 }
private: System::Void bordersCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 script->setBorders(bordersCheckBox->Checked);
		 }
private: System::Void terrainCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 script->setTerrain(terrainCheckBox->Checked);
		 }
private: System::Void buildingsCheckBox_CheckedChanged(System::Object^  sender, System::EventArgs^  e) {

			 script->setBuildings(buildingsCheckBox->Checked, false);
		 }
};
}
