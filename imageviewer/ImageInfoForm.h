#pragma once

#include "MediaInfoForm.h"
#include "FileMetaData.h"
#include "ImageUtils.h"

namespace imageviewer {

using namespace System;
using namespace System::IO;
using namespace System::Text::RegularExpressions;


public ref class ImageInfoForm : public MediaInfoForm
{

private:

	List<FileMetaData ^> ^metaDatas;
	String ^metaDataError;
	bool useDefaultThumb;
	bool deleteThumbs;
	bool useBrowsedThumb;

	void updateMetaData_Event(System::Object^  sender, System::EventArgs^  e) {

		try {

			for each(FileMetaData ^metaData in metaDatas) {

				metaData->Creator = Author;
				metaData->CreatorTool = CreatorTool;
				metaData->Title = Title;
				metaData->Description = Description;
				metaData->Copyright = Copyright;

				MetaDataThumb ^thumbNail = nullptr;

				if(useDefaultThumb) {

					thumbNail = generateThumbnail(metaData->FilePath);

				} else if(deleteThumbs) {

					metaData->Thumbnail->Clear();

				} else if(useBrowsedThumb) {

					thumbNail = Thumbnail;

				}

				if(thumbNail != nullptr) {

					if(metaData->Thumbnail->Count == 0) {

						metaData->Thumbnail->Add(thumbNail);

					} else {

						metaData->Thumbnail[0] = thumbNail;
					}
				}

				if(IsBatch) {

					for each(String ^tag in AddTags) {

						if(!metaData->Tags->Contains(tag)) {

							metaData->Tags->Add(tag);
						}
					}

					for each(String ^tag in RemoveTags) {

						if(metaData->Tags->Contains(tag)) {

							metaData->Tags->Remove(tag);
						}
					}

				} else {

					metaData->Tags = Tags;
				}
					
				

				metaData->saveToDisk();
			}

		} catch (Exception ^e) {

			MessageBox::Show(e->Message, "Error");

		} finally {

			Form::Close();
		}
			
	}

	void imageInfoForm_FormClosed(System::Object^  sender, FormClosedEventArgs^  e) {

		for each(FileMetaData ^metaData in metaDatas) {

			delete metaData;
		}
	}
	
	void imageInfoForm_Shown(System::Object^  sender, EventArgs^  e) {

		if(!String::IsNullOrEmpty(metaDataError)) {

			MessageBox::Show("Cannot open metadata for: \n" + metaDataError, "Error");
			Close();
		}
	}

	MetaDataThumb ^generateThumbnail(String ^path) {

		Image ^fullImage = gcnew Bitmap(path);

		int resizedHeight, resizedWidth;

		ImageUtils::resizeRectangle(fullImage->Width, fullImage->Height, 160, 160, resizedHeight, resizedWidth);
	
		Image ^thumbnail = ImageUtils::resizeImage(fullImage, resizedHeight, resizedWidth);

		delete fullImage;

		return(gcnew MetaDataThumb(thumbnail));
	}

	void setDefaultThumb_Event(System::Object^  sender, GEventArgs<bool> ^e) {

		useDefaultThumb = e->Value;

		if(IsBatch == false && e->Value == true) {
		
			try {

				Thumbnail = generateThumbnail(metaDatas[0]->FilePath);

			} catch (Exception ^e) {

				MessageBox::Show(e->Message);
			}

		}
	}

	void deleteThumbs_Event(System::Object^  sender, GEventArgs<bool> ^e) {

		deleteThumbs = e->Value;

		if(e->Value == true) {
		
			Thumbnail = nullptr;
		}
	}

	void dontChangeThumbs_Event(System::Object^  sender, GEventArgs<bool> ^e) {

		if(e->Value == true) {
		
			if(IsBatch) {

				Thumbnail = nullptr;

			} else {

				if(metaDatas[0]->Thumbnail->Count > 0) {

					Thumbnail = metaDatas[0]->Thumbnail[0];

				} else {

					Thumbnail = nullptr;
				}
			}
		}

	
	}

	void browseThumb_Event(System::Object^ sender, GEventArgs<bool> ^e) {

		if((useBrowsedThumb = e->Value) == false) return;

		OpenFileDialog ^openFileDialog = ImageUtils::createOpenImageFileDialog();

		if(openFileDialog->ShowDialog() == ::DialogResult::OK)
		{			
			try {

				Thumbnail = generateThumbnail(openFileDialog->FileName);					

			} catch (Exception ^e) {

				MessageBox::Show(e->Message);
			}

		} else {

			Thumbnail = nullptr;
		}
	}

public: 
	
	ImageInfoForm() {

		OkButtonClick += gcnew EventHandler<EventArgs ^>(this, &ImageInfoForm::updateMetaData_Event);
		DefaultThumbRadioButtonCheckedChanged += gcnew EventHandler<GEventArgs<bool> ^>(this, &ImageInfoForm::setDefaultThumb_Event);
		BrowseThumbRadioButtonCheckedChanged += gcnew EventHandler<GEventArgs<bool> ^>(this, &ImageInfoForm::browseThumb_Event);
		DeleteThumbRadioButtonCheckedChanged += gcnew EventHandler<GEventArgs<bool> ^>(this, &ImageInfoForm::deleteThumbs_Event);
		NoChangeThumbRadioButtonCheckedChanged += gcnew EventHandler<GEventArgs<bool> ^>(this, &ImageInfoForm::dontChangeThumbs_Event);
		FormClosed += gcnew FormClosedEventHandler(this, &ImageInfoForm::imageInfoForm_FormClosed);
		Shown += gcnew EventHandler(this, &ImageInfoForm::imageInfoForm_Shown);

		metaDatas = gcnew List<FileMetaData ^>();

		metaDataError = nullptr;
		useDefaultThumb = false;
	}
	
	property List<String ^> ^FileNames {

		void set(List<String ^> ^fileNames) { 

			metaDatas->Clear();

			for each(String ^fileName in fileNames) {

				metaDatas->Add(gcnew FileMetaData(fileName));

			}
			
			if(metaDatas->Count == 1) {

				Text = Path::GetFileName(fileNames[0]) + " - Metadata";
				IsBatch = false;

				if(metaDatas[0]->Thumbnail->Count > 0) {

					Thumbnail = metaDatas[0]->Thumbnail[0];
				}

			} else {

				Text = Convert::ToString(metaDatas->Count) + " files - Combined Metadata";
				IsBatch = true;
			}


			MetaDataTreeNode ^tree = metaDatas[0]->Tree;

			for(int i = 1; i < metaDatas->Count; i++) {

				MetaDataTreeNode ^tempTree = metaDatas[i]->Tree;

				tree = tree->intersection(tempTree);

			}			

			MetaDataTreeArray ^arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:title"));
			if(arr != nullptr && arr->Count > 0) {
				Title = arr[0]->Data;
			}

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:description"));
			if(arr != nullptr && arr->Count > 0) {
				Description = arr[0]->Data;
			}

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:creator"));
			if(arr != nullptr && arr->Count > 0) {
				Author = arr[0]->Data;
			}

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:rights"));
			if(arr != nullptr && arr->Count > 0) {
				Copyright = arr[0]->Data;
			}

			MetaDataTreeProperty ^prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:CreatorTool"));
			if(prop != nullptr) {
				CreatorTool = prop->Value;
			}
			
			prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:MetadataDate"));
			if(prop != nullptr) {
				MetaDataDate = MetaData::convertToDate(prop->Value);
			} else {
				MetaDataDate = DateTime::MinValue;
			}

			prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:CreateDate"));
			if(prop != nullptr) {
				CreateDate = MetaData::convertToDate(prop->Value);
			} else {
				CreateDate = DateTime::MinValue;
			}

			prop = dynamic_cast<MetaDataTreeProperty ^>(tree->getNode("xmp:ModifyDate"));
			if(prop != nullptr) {
				ModifyDate = MetaData::convertToDate(prop->Value);
			} else {
				ModifyDate = DateTime::MinValue;
			}

			List<String ^> ^tags = gcnew List<String ^>();

			arr = dynamic_cast<MetaDataTreeArray ^>(tree->getNode("dc:subject"));
			if(arr != nullptr) {
				
				for each(MetaDataTreeNode ^n in arr->Child) {

					tags->Add(n->Data);
				}
			}

			Misc = tree;				
			Tags = tags;

			for(int i = 0; i < metaDatas->Count; i++) {

				metaDatas[i]->closeFile();

			}			

		}

		List<String ^> ^get() {

			List<String ^> ^fileNames = gcnew List<String ^>();

			for each(FileMetaData ^metaData in metaDatas) {

				fileNames->Add(metaData->FilePath);
			}

			return(fileNames);

		}
	}

	
	
};

}