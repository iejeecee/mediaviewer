#pragma once
//http://msdn.microsoft.com/en-us/library/fbk67b6z.aspx

#include "Database.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;

using namespace System::Data::SqlClient;
using namespace System::Data::SqlServerCe;



namespace imageviewer {

	/// <summary>
	/// Summary for TagEditorForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class TagEditorForm : public System::Windows::Forms::Form
	{
	public:
		TagEditorForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//

			bindingSource = gcnew BindingSource();
			dataAdapter = gcnew SqlCeDataAdapter();
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~TagEditorForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::DataGridView^  dataGridView;
	private: System::Windows::Forms::Button^  updateButton;
	private: System::Windows::Forms::Button^  cancelButton;
	protected: 

	protected: 


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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(TagEditorForm::typeid));
			this->dataGridView = (gcnew System::Windows::Forms::DataGridView());
			this->updateButton = (gcnew System::Windows::Forms::Button());
			this->cancelButton = (gcnew System::Windows::Forms::Button());
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->dataGridView))->BeginInit();
			this->SuspendLayout();
			// 
			// dataGridView
			// 
			this->dataGridView->ColumnHeadersHeightSizeMode = System::Windows::Forms::DataGridViewColumnHeadersHeightSizeMode::AutoSize;
			this->dataGridView->Location = System::Drawing::Point(13, 13);
			this->dataGridView->Name = L"dataGridView";
			this->dataGridView->RowTemplate->Height = 28;
			this->dataGridView->Size = System::Drawing::Size(451, 386);
			this->dataGridView->TabIndex = 0;
			// 
			// updateButton
			// 
			this->updateButton->Location = System::Drawing::Point(12, 411);
			this->updateButton->Name = L"updateButton";
			this->updateButton->Size = System::Drawing::Size(94, 45);
			this->updateButton->TabIndex = 1;
			this->updateButton->Text = L"Update";
			this->updateButton->UseVisualStyleBackColor = true;
			this->updateButton->Click += gcnew System::EventHandler(this, &TagEditorForm::updateButton_Click);
			// 
			// cancelButton
			// 
			this->cancelButton->DialogResult = System::Windows::Forms::DialogResult::Cancel;
			this->cancelButton->Location = System::Drawing::Point(112, 411);
			this->cancelButton->Name = L"cancelButton";
			this->cancelButton->Size = System::Drawing::Size(94, 45);
			this->cancelButton->TabIndex = 2;
			this->cancelButton->Text = L"Cancel";
			this->cancelButton->UseVisualStyleBackColor = true;
			// 
			// TagEditorForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->CancelButton = this->cancelButton;
			this->ClientSize = System::Drawing::Size(476, 468);
			this->Controls->Add(this->cancelButton);
			this->Controls->Add(this->updateButton);
			this->Controls->Add(this->dataGridView);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"TagEditorForm";
			this->Text = L"Tag Editor";
			this->Load += gcnew System::EventHandler(this, &TagEditorForm::TagEditorForm_OnLoad);
			(cli::safe_cast<System::ComponentModel::ISupportInitialize^  >(this->dataGridView))->EndInit();
			this->ResumeLayout(false);

		}
#pragma endregion

	private: BindingSource^ bindingSource;
	private: SqlCeDataAdapter^ dataAdapter;

			 void GetData(String^ selectCommand)
			 {
				 try
				 {
					
					 // Create a new data adapter based on the specified query.
					 dataAdapter = gcnew SqlCeDataAdapter(selectCommand, Database::Connection);

					 // Create a command builder to generate SQL update, insert, and 
					 // delete commands based on selectCommand. These are used to 
					 // update the database. 
					 gcnew SqlCeCommandBuilder(dataAdapter);

					 // Populate a new data table and bind it to the BindingSource.
					 DataTable^ table = gcnew DataTable();
					 dataAdapter->Fill(table);
					 bindingSource->DataSource = table;

					 // Resize the DataGridView columns to fit the newly loaded content.
					 dataGridView->AutoResizeColumns( 
						 DataGridViewAutoSizeColumnsMode::AllCellsExceptHeader);
		
				 }
				 catch (SqlException^)
				 {
					 MessageBox::Show("Database Error");
				 }
			 }
	private: System::Void TagEditorForm_OnLoad(System::Object^  sender, System::EventArgs^  e) {

				dataGridView->DataSource = bindingSource;
				GetData("select * from Tags");
			 }
private: System::Void updateButton_Click(System::Object^  sender, System::EventArgs^  e) {

			 dataAdapter->Update((DataTable^)bindingSource->DataSource);
		 }

};
}
