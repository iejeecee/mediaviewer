#pragma once

#include "ProgressForm.h"
#include "ImageStream.h"
#include "ConcurrentList.h"
#include "PicasaPhotoForm.h"
#include "PicasaCommentForm.h"

using namespace Google::GData::Photos;
using namespace Google::GData::Client;
using namespace Google::Picasa;

namespace imageviewer {

	public ref class PicasaAsyncState : public EventArgs
	{
	public:

		enum class OperationType {

			PHOTO_QUERY,
			ALBUM_QUERY,
			COMMENT_QUERY,
			ADD_COMMENT,
			INSERT_ALBUM,
			UPLOAD_PHOTO,
			DELETE_PHOTO,
			DELETE_ALBUM

		} type;

		bool cancel;

		// upload images state
		imageviewer::ProgressForm ^progressForm;
		Uri ^uploadUri;
		List<ImageStream ^> ^uploadImages;
		int uploadImageNr;
		AtomId ^feedId;

		// Album Query state
		String ^userName;
		String ^openAlbumId;

		// Comment query state
		PicasaCommentForm ^commentForm;

		// delete images state
		List<Photo ^> ^deleteImages;
		
		// delete album state
		List<Album ^> ^deleteAlbums;

		int deletedEntry;

		PicasaAsyncState(OperationType type) {

			this->type = type;
			cancel = false;

			if(type == OperationType::UPLOAD_PHOTO) {

				progressForm = gcnew ProgressForm();
				progressForm->Text = "Picasa Upload";
				progressForm->itemProgressMaximum = 100;
				uploadImageNr = 0;
				feedId = nullptr;		
			}

			if(type == OperationType::ALBUM_QUERY) {

				userName = L"";
				openAlbumId = L"";
			}

			if(type == OperationType::DELETE_PHOTO) {

				progressForm = gcnew ProgressForm();
				progressForm->Text = "Picasa Delete Photo";
				progressForm->itemProgressMaximum = 100;
				progressForm->userState = this;
				deleteImages = gcnew List<Photo ^>();

			}

			if(type == OperationType::DELETE_ALBUM) {

				deleteAlbums = gcnew List<Album ^>();
			}

			deletedEntry = 0;		
		}

	};

	public ref class PicasaAsyncStateList : public ConcurrentList<PicasaAsyncState ^> {

	public:

		PicasaAsyncState ^GetType(PicasaAsyncState::OperationType type) {

			Monitor::Enter(lock);

			try
			{
				for(int i = 0; i < list->Count; i++) {

					if(list[i]->type == type) {

						return(list[i]);
					}
				}

				return(nullptr);
			}
			finally
			{
				Monitor::Exit(lock);
			}

		}

	};

}