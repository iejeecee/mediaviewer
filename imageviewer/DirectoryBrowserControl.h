#pragma once

#include "DirectoryBrowserAsyncState.h"
#include "InputDialog.h"
#include "FileUtils.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections::Specialized;
using namespace System::Collections::Generic;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace System::IO;


namespace imageviewer {

	/// <summary>
	/// Summary for DirectoryBrowserControl
	/// </summary>
	public ref class DirectoryBrowserControl : public System::Windows::Forms::UserControl
	{
	public: 

		delegate void AfterSelectDelegate(System::Object^  sender, System::Windows::Forms::TreeViewEventArgs^  e);
		event AfterSelectDelegate ^OnAfterSelect;

		enum class ClipboardAction {
			CUT,
			COPY			
		};

		static ClipboardAction clipboardAction = ClipboardAction::COPY;

		DirectoryBrowserControl(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			createDrivesTreeNodes();
			ignoreSelectNode = true;
			expandFinished = gcnew AutoResetEvent(false);
			initialDirectory = "";
		}

	protected:

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~DirectoryBrowserControl()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TreeView^  directoryTreeView;
	private: System::Windows::Forms::ImageList^  directoryImageList;
	private: System::ComponentModel::IContainer^  components;
	private: System::Windows::Forms::ContextMenuStrip^  contextMenuStrip;
	private: System::Windows::Forms::ToolStripMenuItem^  createDirectoryToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  deleteDirectoryToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  renameDirectoryToolStripMenuItem;

	private: System::ComponentModel::BackgroundWorker^  buildTreeBW;
	private: System::ComponentModel::BackgroundWorker^  createDirectoryChildNodesBW;


	private: List<DriveInfo ^> ^drives;
	private: delegate void treeNodeExpandDelegate(TreeNode ^node);

	private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator1;

	private: System::Windows::Forms::ToolStripMenuItem^  cutToolStripMenuItem;
	private: System::Windows::Forms::ToolStripMenuItem^  pasteToolStripMenuItem;
	private: System::Windows::Forms::ToolStripSeparator^  toolStripSeparator3;
	private: System::Windows::Forms::ToolStripMenuItem^  copyToolStripMenuItem;
	private: bool ignoreSelectNode;
    private: delegate void ModifyTreeDelegate(TreeNode ^parent, TreeNode ^child);
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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(DirectoryBrowserControl::typeid));
			this->directoryTreeView = (gcnew System::Windows::Forms::TreeView());
			this->directoryImageList = (gcnew System::Windows::Forms::ImageList(this->components));
			this->contextMenuStrip = (gcnew System::Windows::Forms::ContextMenuStrip(this->components));
			this->createDirectoryToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripSeparator1 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->cutToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->copyToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->pasteToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->toolStripSeparator3 = (gcnew System::Windows::Forms::ToolStripSeparator());
			this->renameDirectoryToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->deleteDirectoryToolStripMenuItem = (gcnew System::Windows::Forms::ToolStripMenuItem());
			this->buildTreeBW = (gcnew System::ComponentModel::BackgroundWorker());
			this->createDirectoryChildNodesBW = (gcnew System::ComponentModel::BackgroundWorker());
			this->contextMenuStrip->SuspendLayout();
			this->SuspendLayout();
			// 
			// directoryTreeView
			// 
			this->directoryTreeView->Dock = System::Windows::Forms::DockStyle::Fill;
			this->directoryTreeView->ImageIndex = 0;
			this->directoryTreeView->ImageList = this->directoryImageList;
			this->directoryTreeView->Location = System::Drawing::Point(0, 0);
			this->directoryTreeView->Name = L"directoryTreeView";
			this->directoryTreeView->SelectedImageIndex = 0;
			this->directoryTreeView->Size = System::Drawing::Size(150, 150);
			this->directoryTreeView->TabIndex = 1;
			this->directoryTreeView->BeforeExpand += gcnew System::Windows::Forms::TreeViewCancelEventHandler(this, &DirectoryBrowserControl::directoryTreeView_BeforeExpand);
			this->directoryTreeView->MouseUp += gcnew System::Windows::Forms::MouseEventHandler(this, &DirectoryBrowserControl::directoryTreeView_MouseUp);
			this->directoryTreeView->VisibleChanged += gcnew System::EventHandler(this, &DirectoryBrowserControl::directoryTreeView_VisibleChanged);
			this->directoryTreeView->AfterSelect += gcnew System::Windows::Forms::TreeViewEventHandler(this, &DirectoryBrowserControl::directoryTreeView_AfterSelect);
			this->directoryTreeView->BeforeSelect += gcnew System::Windows::Forms::TreeViewCancelEventHandler(this, &DirectoryBrowserControl::directoryTreeView_BeforeSelect);
			// 
			// directoryImageList
			// 
			this->directoryImageList->ImageStream = (cli::safe_cast<System::Windows::Forms::ImageListStreamer^  >(resources->GetObject(L"directoryImageList.ImageStream")));
			this->directoryImageList->TransparentColor = System::Drawing::Color::Transparent;
			this->directoryImageList->Images->SetKeyName(0, L"Hard_Drive.ico");
			this->directoryImageList->Images->SetKeyName(1, L"CD_Drive.ico");
			this->directoryImageList->Images->SetKeyName(2, L"dvddrive.ico");
			this->directoryImageList->Images->SetKeyName(3, L"Network_Drive.ico");
			this->directoryImageList->Images->SetKeyName(4, L"3.5_Disk_Drive.ico");
			this->directoryImageList->Images->SetKeyName(5, L"525_DiskDrive.ico");
			this->directoryImageList->Images->SetKeyName(6, L"Folder_Back.ico");
			this->directoryImageList->Images->SetKeyName(7, L"folder_open.ico");
			this->directoryImageList->Images->SetKeyName(8, L"UnknownDrive.ico");
			this->directoryImageList->Images->SetKeyName(9, L"computer.ico");
			// 
			// contextMenuStrip
			// 
			this->contextMenuStrip->Items->AddRange(gcnew cli::array< System::Windows::Forms::ToolStripItem^  >(8) {this->createDirectoryToolStripMenuItem, 
				this->toolStripSeparator1, this->cutToolStripMenuItem, this->copyToolStripMenuItem, this->pasteToolStripMenuItem, this->toolStripSeparator3, 
				this->renameDirectoryToolStripMenuItem, this->deleteDirectoryToolStripMenuItem});
			this->contextMenuStrip->Name = L"contextMenuStrip";
			this->contextMenuStrip->Size = System::Drawing::Size(168, 172);
			// 
			// createDirectoryToolStripMenuItem
			// 
			this->createDirectoryToolStripMenuItem->Name = L"createDirectoryToolStripMenuItem";
			this->createDirectoryToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->createDirectoryToolStripMenuItem->Text = L"Create New";
			this->createDirectoryToolStripMenuItem->Click += gcnew System::EventHandler(this, &DirectoryBrowserControl::createDirectoryToolStripMenuItem_Click);
			// 
			// toolStripSeparator1
			// 
			this->toolStripSeparator1->Name = L"toolStripSeparator1";
			this->toolStripSeparator1->Size = System::Drawing::Size(164, 6);
			// 
			// cutToolStripMenuItem
			// 
			this->cutToolStripMenuItem->Name = L"cutToolStripMenuItem";
			this->cutToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->cutToolStripMenuItem->Text = L"Cut";
			this->cutToolStripMenuItem->Click += gcnew System::EventHandler(this, &DirectoryBrowserControl::cutToolStripMenuItem_Click);
			// 
			// copyToolStripMenuItem
			// 
			this->copyToolStripMenuItem->Name = L"copyToolStripMenuItem";
			this->copyToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->copyToolStripMenuItem->Text = L"Copy";
			this->copyToolStripMenuItem->Click += gcnew System::EventHandler(this, &DirectoryBrowserControl::copyToolStripMenuItem_Click);
			// 
			// pasteToolStripMenuItem
			// 
			this->pasteToolStripMenuItem->Name = L"pasteToolStripMenuItem";
			this->pasteToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->pasteToolStripMenuItem->Text = L"Paste";
			this->pasteToolStripMenuItem->Click += gcnew System::EventHandler(this, &DirectoryBrowserControl::pasteToolStripMenuItem_Click);
			// 
			// toolStripSeparator3
			// 
			this->toolStripSeparator3->Name = L"toolStripSeparator3";
			this->toolStripSeparator3->Size = System::Drawing::Size(164, 6);
			// 
			// renameDirectoryToolStripMenuItem
			// 
			this->renameDirectoryToolStripMenuItem->Name = L"renameDirectoryToolStripMenuItem";
			this->renameDirectoryToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->renameDirectoryToolStripMenuItem->Text = L"Rename";
			this->renameDirectoryToolStripMenuItem->Click += gcnew System::EventHandler(this, &DirectoryBrowserControl::renameDirectoryToolStripMenuItem_Click);
			// 
			// deleteDirectoryToolStripMenuItem
			// 
			this->deleteDirectoryToolStripMenuItem->Name = L"deleteDirectoryToolStripMenuItem";
			this->deleteDirectoryToolStripMenuItem->Size = System::Drawing::Size(167, 26);
			this->deleteDirectoryToolStripMenuItem->Text = L"Delete";
			this->deleteDirectoryToolStripMenuItem->Click += gcnew System::EventHandler(this, &DirectoryBrowserControl::deleteDirectoryToolStripMenuItem_Click);
			// 
			// buildTreeBW
			// 
			this->buildTreeBW->DoWork += gcnew System::ComponentModel::DoWorkEventHandler(this, &DirectoryBrowserControl::buildTreeBW_DoWork);
			this->buildTreeBW->RunWorkerCompleted += gcnew System::ComponentModel::RunWorkerCompletedEventHandler(this, &DirectoryBrowserControl::buildTreeBW_RunWorkerCompleted);
			// 
			// createDirectoryChildNodesBW
			// 
			this->createDirectoryChildNodesBW->DoWork += gcnew System::ComponentModel::DoWorkEventHandler(this, &DirectoryBrowserControl::createDirectoryChildNodesBW_DoWork);
			this->createDirectoryChildNodesBW->RunWorkerCompleted += gcnew System::ComponentModel::RunWorkerCompletedEventHandler(this, &DirectoryBrowserControl::createDirectoryChildNodesBW_RunWorkerComplete);
			// 
			// DirectoryBrowserControl
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->Controls->Add(this->directoryTreeView);
			this->Name = L"DirectoryBrowserControl";
			this->contextMenuStrip->ResumeLayout(false);
			this->ResumeLayout(false);

		}
#pragma endregion

private:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	AutoResetEvent ^expandFinished;
	String ^initialDirectory;

	public: event System::IO::RenamedEventHandler ^Renamed;

	private: void findDrives() {

				 drives = gcnew List<DriveInfo ^>();

				 cli::array<DriveInfo ^> ^drivesArray = DriveInfo::GetDrives();

				 for each (DriveInfo ^drive in drivesArray)
				 {
					 drives->Add(drive);
				 }
			 }

	private: TreeNode ^driveToTreeNode(DriveInfo ^drive) {

				 String ^name;
				 int imageIndex;
				 int selectedImageIndex;

				 switch(drive->DriveType) {

					 case DriveType::Fixed: 
						 {

							 name = drive->VolumeLabel;
							 imageIndex = 0;
							 selectedImageIndex = 0;						
							 break;
						 }
					 case DriveType::Ram: 
						 {

							 name = "Ram Drive";
							 imageIndex = 0;
							 selectedImageIndex = 0;
							 break;
						 }
					 case DriveType::CDRom: 
						 {

							 name = "CD Rom";
							 imageIndex = 1;
							 selectedImageIndex = 1;
							 break;
						 }
					 case DriveType::Network: 
						 {

							 name = drive->VolumeLabel;
							 imageIndex = 3;
							 selectedImageIndex = 3;
							 break;
						 }
					 case DriveType::Removable: 
						 {

							 name = "Removable Drive";
							 imageIndex = 4;
							 selectedImageIndex = 4;
							 break;
						 }
					 default: 
						 {

							 name = "Unknown Drive";
							 imageIndex = 7;
							 selectedImageIndex = 7;
							 break;
						 }

				 }

				 String ^driveName = drive->Name->Replace("\\", "");

				 name = name + " (" + driveName + ")";

				 TreeNode ^driveNode = gcnew TreeNode(name, imageIndex, selectedImageIndex);				

				 DirectoryInfo ^tag = gcnew DirectoryInfo(drive->Name);

				 driveNode->Tag = tag;
				 driveNode->Name = tag->FullName;

				 driveNode->Nodes->Add(createDummyNode());

				 return(driveNode);
			 }

	private: TreeNode ^createDummyNode() {

				 TreeNode ^dummy = gcnew TreeNode("Loading..",6,6);
				 dummy->Tag = nullptr;

				 return(dummy);
			 }
	private: TreeNode ^createDirectoryNode(DirectoryInfo ^directory) {

				 TreeNode ^directoryNode = gcnew TreeNode(directory->Name, 6, 7);
				 directoryNode->Name = directory->FullName;
				 directoryNode->Tag = directory;

				 return(directoryNode);
			 }

	private: void createDrivesTreeNodes() {

				 findDrives();

				 for(int i = 0; i < drives->Count; i++) {

					 DriveInfo ^drive = drives[i];

					 TreeNode ^driveNode = driveToTreeNode(drive);	
					 //rootNode->Nodes->Add(driveNode);
					 directoryTreeView->Nodes->Add(driveNode);

				 }

			 }

	public: void createDirectoryTreeViewFromPath(String ^path) {

				if(directoryTreeView->Created == false) {

					// Events will not be fired when treeview is not created.
					// Instead the initial directory will be set once the treeview visible 
					// state changes to true
					initialDirectory = path;
					return;
				}

				if(!isDriveReady(gcnew DirectoryInfo(path))) return;

				cli::array<TreeNode ^> ^result = directoryTreeView->Nodes->Find(path, true);

				if(result->Length != 0) {

					directoryTreeView->SelectedNode = result[0];

				} else {

					directoryTreeView->CollapseAll();			
					buildTreeBW->RunWorkerAsync(path);
				}


			}

	private: bool isPathInDirectoryTreeView(String ^path) {

				 cli::array<TreeNode ^> ^result = directoryTreeView->Nodes->Find(path, true);

				 if(result->Length == 0) {

					 return(false);

				 } else {

					 return(true);
				 }

			 }
	private: System::Void directoryTreeView_AfterSelect(System::Object^  sender, System::Windows::Forms::TreeViewEventArgs^  e) {


				 OnAfterSelect(sender, e);

			 }
	private: System::Void directoryTreeView_BeforeExpand(System::Object^  sender, System::Windows::Forms::TreeViewCancelEventArgs^  e) {

				 DirectoryInfo ^directory = dynamic_cast<DirectoryInfo ^>(e->Node->Tag);

				 if(!isDriveReady(directory)) {

					 e->Cancel = true;
					 return;
				 }

				 DirectoryBrowserAsyncState ^state = gcnew DirectoryBrowserAsyncState();

				 state->parent = e->Node;
				 state->path = directory->FullName;

				 // wait until the asyncworker is available
				 while(createDirectoryChildNodesBW->IsBusy == true) {

					 Application::DoEvents();
				 }

				 createDirectoryChildNodesBW->RunWorkerAsync(state);

			 }
	private: System::Void directoryTreeView_BeforeSelect(System::Object^  sender, System::Windows::Forms::TreeViewCancelEventArgs^  e) {

				 DirectoryInfo ^directory = dynamic_cast<DirectoryInfo ^>(e->Node->Tag);

				 if(directory == nullptr || (ignoreSelectNode && (e->Action == TreeViewAction::Unknown))) {

					 // a dummy node was selected or a unwanted selection is fired
					 // during initialization of the tree
					 e->Cancel = true;
					 return;

				 } 

				 if(!isDriveReady(directory)) {

					 e->Cancel = true;
					 return;
				 }

			 }
	private: bool isDriveReady(DirectoryInfo ^directory) {

				 DriveInfo ^drive = gcnew DriveInfo(directory->Root->Name);

				 if(drive->IsReady == false) {

					 MessageBox::Show("Unable to read from drive: " + drive->Name, "Error");

					 return(false);
				 }

				 return(true);
			 }

	private: System::Void createDirectoryToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 DirectoryInfo ^directory = dynamic_cast<DirectoryInfo ^>(directoryTreeView->SelectedNode->Tag);

				 InputDialog ^dialog = gcnew InputDialog();
				 dialog->Text = "Create directory at: " + directory->FullName;

				 if(dialog->ShowDialog() == ::DialogResult::OK) {

					 try {

						 String ^newDir = directory->FullName + "\\" + dialog->inputText;

						 Directory::CreateDirectory(newDir);

						 TreeNode ^newDirNode = createDirectoryNode(gcnew DirectoryInfo(newDir));

						 directoryTreeView->SelectedNode->Nodes->Add(newDirNode);

						 directoryTreeView->SelectedNode->Expand();

					 } catch (Exception ^e) {

						 log->Error("Error Creating Directory", e);
						 MessageBox::Show(e->Message, "Error Creating Directory");
					 }
				 }

			 }
	private: System::Void deleteDirectoryToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {
				  
				 TreeNode ^deleteNode = directoryTreeView->SelectedNode;
				 // root nodes are drives
				 if(deleteNode->Parent == nullptr) return;

				 DirectoryInfo ^directory = dynamic_cast<DirectoryInfo ^>(deleteNode->Tag);

				 try {

					 if(MessageBox::Show("Delete:\n\n" + directory->FullName + "\n\nand ALL it's subdirectories?", "Warning!",
						 MessageBoxButtons::YesNoCancel, MessageBoxIcon::Warning) == ::DialogResult::Yes) {

							 Directory::Delete(directory->FullName, true);

							 deleteNode->Parent->Nodes->Remove(deleteNode);

					 }

				 } catch (Exception ^e) {

					 log->Error("Error Deleting Directory", e);
					 MessageBox::Show(e->Message, "Error Deleting Directory");
				 }
			 }


	private: System::Void treeNodeExpandInvoker(TreeNode ^parent) {

				 parent->Expand();
									 
			 }

	private: System::Void buildTreeBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {

				 String ^path = dynamic_cast<String ^>(e->Argument);

				 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(1);
				 treeNodeExpandDelegate ^expandTreeNode = gcnew treeNodeExpandDelegate(this, &DirectoryBrowserControl::treeNodeExpandInvoker);
				 TreeNode ^parentNode;

				 String ^curPath = System::IO::Path::GetPathRoot(path);

				 cli::array<TreeNode ^> ^result = directoryTreeView->Nodes->Find(curPath, false);

				 if(result->Length == 0) return;

				 args[0] = parentNode = result[0];

				 expandFinished->Reset(); 
				 directoryTreeView->Invoke(expandTreeNode, args);
				 expandFinished->WaitOne();

				 String ^seperator = L"\\";

				 cli::array<String ^> ^splitDirs = path->Split(seperator->ToCharArray());

				 for(int i = 1; i < splitDirs->Length - 1; i++) {

					 curPath += splitDirs[i];

					 cli::array<TreeNode ^> ^result = parentNode->Nodes->Find(curPath, false);

					 if(result->Length == 0) return;

					 args[0] = parentNode = result[0];

					 directoryTreeView->Invoke(expandTreeNode, args);
					 expandFinished->WaitOne();

					 curPath += "\\";

				 }

				 e->Result = path;

			 }

	private: System::Void buildTreeBW_RunWorkerCompleted(System::Object^  sender, System::ComponentModel::RunWorkerCompletedEventArgs^  e) {

				 String ^path = dynamic_cast<String ^>(e->Result);

				 cli::array<TreeNode ^> ^result = directoryTreeView->Nodes->Find(path, true);

				 if(result->Length == 0) return;

				 ignoreSelectNode = false;
				 directoryTreeView->SelectedNode = result[0];
			 }

	private: System::Void createDirectoryChildNodesBW_DoWork(System::Object^  sender, System::ComponentModel::DoWorkEventArgs^  e) {

				 DirectoryBrowserAsyncState ^state = dynamic_cast<DirectoryBrowserAsyncState ^>(e->Argument);

				 try {					 

					 DirectoryInfo ^info = gcnew DirectoryInfo(state->path);

					 cli::array<DirectoryInfo ^> ^directories = info->GetDirectories();

					 for each(DirectoryInfo ^directory in directories) {

						 DirectoryInfo ^subDir = gcnew DirectoryInfo(state->path + "\\" + directory->Name);
						 if((directory->Attributes & FileAttributes::System) == FileAttributes::System) continue;

						 if(subDir->GetDirectories()->Length > 0) {

							 state->hasSubDirs->Add(true);

						 } else {

							 state->hasSubDirs->Add(false);
						 }

						 state->directories->Add(directory);
					 }

					 e->Result = state;

				 } catch (Exception ^ex) {

					 log->Error("Error expanding directory tree", ex);

					 // don't block in case of error
					 expandFinished->Set();
					 
				 } 

			 }

	private: System::Void createDirectoryChildNodesBW_RunWorkerComplete(System::Object^  sender, System::ComponentModel::RunWorkerCompletedEventArgs^  e) {

				 try {

					 DirectoryBrowserAsyncState ^state = dynamic_cast<DirectoryBrowserAsyncState ^>(e->Result);

					 state->parent->Nodes->Clear();

					 for(int i = 0; i < state->directories->Count; i++) {

						 DirectoryInfo ^directory = state->directories[i];
						 bool hasSubDirs = state->hasSubDirs[i];					 

						 TreeNode ^directoryNode = createDirectoryNode(directory);

						 if(hasSubDirs) {

							 directoryNode->Nodes->Add(createDummyNode());
						 }

						 state->parent->Nodes->Add(directoryNode);

					 }

				 } finally {

					 expandFinished->Set();
				 }
				 
			 }

	private: System::Void renameDirectoryToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

				 TreeNode ^renameNode = directoryTreeView->SelectedNode;

				 // root nodes are drives
				 if(renameNode == nullptr || renameNode->Parent == nullptr) return;

				 DirectoryInfo ^directory = dynamic_cast<DirectoryInfo ^>(renameNode->Tag);

				 InputDialog ^dialog = gcnew InputDialog();
				 dialog->Text = "Rename Directory: " + directory->FullName;
				 dialog->inputText = directory->Name;

				 if(dialog->ShowDialog() == ::DialogResult::OK) {

					 try {

						 String ^oldPath = directory->FullName;
						 String ^newPath = oldPath->Remove(oldPath->LastIndexOf("\\") + 1) + dialog->inputText;

						 Directory::Move(oldPath, newPath);						 

						 TreeNode ^newNode = createDirectoryNode(gcnew DirectoryInfo(newPath));
						 for each(TreeNode ^node in renameNode->Nodes) {

							 TreeNode ^clone = dynamic_cast<TreeNode ^>(node->Clone());

							 newNode->Nodes->Add(clone);
						 }

						 TreeNode ^parent = renameNode->Parent;
						 int index = parent->Nodes->IndexOf(renameNode);
						 ignoreSelectNode = true;
						 parent->Nodes->Remove(renameNode);

						 parent->Nodes->Insert(index, newNode);
						 ignoreSelectNode = false;

						 RenamedEventArgs ^args = gcnew RenamedEventArgs(WatcherChangeTypes::Renamed, Path::GetDirectoryName(oldPath), dialog->inputText, Path::GetFileName(oldPath));

						 Renamed(this, args);
						
					 } catch (Exception ^e) {

						 log->Error("Error Renaming Directory", e);
						 MessageBox::Show(e->Message, "Error Renaming Directory");
					 }
				 }

			 }
private: System::Void cutToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

			 TreeNode ^selectedNode = directoryTreeView->SelectedNode;

			 StringCollection ^path = gcnew StringCollection();
			 path->Add(selectedNode->Name);

			 Clipboard::SetFileDropList(path);

			 clipboardAction = ClipboardAction::CUT;

		 }
private: System::Void pasteToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {
			
			 if(!Clipboard::ContainsFileDropList()) return;

			 TreeNode ^selectedNode = directoryTreeView->SelectedNode;
			
			 StringCollection ^sourcePaths = Clipboard::GetFileDropList();

	     	 StringCollection ^destPaths = gcnew StringCollection();
		
			 for each(String ^sourcePath in sourcePaths) {

				 String ^fileName = System::IO::Path::GetFileName(sourcePath);
				 String ^destPath = selectedNode->Name + "\\" + fileName;

				 destPaths->Add(destPath);
			 }

			 FileUtils ^fileUtils = gcnew FileUtils();
			 fileUtils->OnAfterCopy += gcnew FileUtils::FileUtilsDelegate(this, &DirectoryBrowserControl::fileUtils_OnAfterCopy);
			 fileUtils->OnAfterDelete += gcnew FileUtils::FileUtilsDelegate(this, &DirectoryBrowserControl::fileUtils_OnAfterDelete);

			 try {

				 if(clipboardAction == ClipboardAction::CUT) {

					fileUtils->move(sourcePaths, destPaths);

				 } else if(clipboardAction == ClipboardAction::COPY) {

					fileUtils->copy(sourcePaths, destPaths);
				 }

			 } catch (Exception ^e) {

				 log->Error("Error pasting files", e);
				 MessageBox::Show(e->Message);
			 }

		 }
 
 private: void fileUtils_OnAfterCopy(System::Object^  sender, FileUtilsEventArgs^  e) {

			  // add newly created directories (and their parents) to the directoryTreeView
			  if(e->isDirectory) {
			     
				 String ^parentName = e->filePath;

				 cli::array<TreeNode ^> ^parentNode = gcnew cli::array<TreeNode ^>(0);
				 List<TreeNode ^> ^children = gcnew List<TreeNode ^>();
				
				 while(parentNode->Length == 0) {

					 children->Insert(0, createDirectoryNode(gcnew DirectoryInfo(parentName)));

					 parentName = System::IO::Path::GetDirectoryName(parentName);

					 parentNode = directoryTreeView->Nodes->Find(parentName, true);

				 }

				 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(2);

				 TreeNode ^parent = parentNode[0];

				 for each(TreeNode ^child in children) {
					
					 args[0] = parent;
					 args[1] = child;

					 this->BeginInvoke(gcnew ModifyTreeDelegate(this, &DirectoryBrowserControl::addNode), args);

					 parent = child;
				 }
			  }
		  }
  private: void fileUtils_OnAfterDelete(System::Object^  sender, FileUtilsEventArgs^  e) {

			   // remove deleted directories to the directoryTreeView
			  if(e->isDirectory) {

				 cli::array<TreeNode ^> ^movedNode = directoryTreeView->Nodes->Find(e->filePath, true);

				 if(movedNode->Length != 0) {

					 cli::array<Object ^> ^args = gcnew cli::array<Object ^>(2);
					 args[0] = movedNode[0]->Parent;;
					 args[1] = movedNode[0];

					 this->BeginInvoke(gcnew ModifyTreeDelegate(this, &DirectoryBrowserControl::removeNode), args);				
				 }
			  }

		  }

  private: void addNode(TreeNode ^parent, TreeNode ^child) {

				parent->Nodes->Add(child);
			}
   private: void removeNode(TreeNode ^parent, TreeNode ^child) {

				parent->Nodes->Remove(child);
			}
  


private: System::Void copyToolStripMenuItem_Click(System::Object^  sender, System::EventArgs^  e) {

			 TreeNode ^selectedNode = directoryTreeView->SelectedNode;

			 StringCollection ^path = gcnew StringCollection();
			 path->Add(selectedNode->Name);

			 Clipboard::SetFileDropList(path);

			 clipboardAction = ClipboardAction::COPY;

		 }
private: System::Void directoryTreeView_MouseUp(System::Object^  sender, System::Windows::Forms::MouseEventArgs^  e) {
			 
			if(e->Button == System::Windows::Forms::MouseButtons::Right)
			{
				// Select the clicked node
				ignoreSelectNode = false;
				directoryTreeView->SelectedNode = directoryTreeView->GetNodeAt(e->X, e->Y);

				if(directoryTreeView->SelectedNode != nullptr)
				{
					contextMenuStrip->Show(directoryTreeView, e->Location);
				}
			}

		 }
private: System::Void directoryTreeView_VisibleChanged(System::Object^  sender, System::EventArgs^  e) {

			 if(directoryTreeView->Visible == true) {

				 if(!String::IsNullOrEmpty(initialDirectory)) {

					 createDirectoryTreeViewFromPath(initialDirectory);
					 initialDirectory = "";
				 }
			 }
		 }
};
}
