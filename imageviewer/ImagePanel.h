#pragma once

// async tutorial
// http://www.codeproject.com/Articles/14931/Asynchronous-Method-Invocation

#include "HttpRequest.h"
#include "MediaFormatConvert.h"
#include "Util.h"
#include "FileUtils.h"
#include "ImageStream.h"
#include "ImageUtils.h"
#include "MediaFileFactory.h"
#include <msclr\lock.h>

#using <System.dll>
#using <System.Drawing.dll>
#using <System.Windows.Forms.dll>
#using <System.Windows.Forms.dll>

using namespace System;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::Drawing::Drawing2D;
using namespace System::Threading;
using namespace System::Windows::Forms;
using namespace System::Diagnostics;

namespace imageviewer {



	public ref class AsyncLoadImageArgs 
	{
	public:

		enum class Mode {

			NORMAL,
			SCALED,
			THUMBNAIL
		};

		String ^imageLocation;
		Mode loadMode;

		ImageStream ^imageStream;

		AsyncLoadImageArgs(String ^imageLocation, Mode loadMode) {

			this->imageLocation = imageLocation;
			this->loadMode = loadMode;

			imageStream = nullptr;
		}

		AsyncLoadImageArgs(ImageStream ^imageStream, Mode loadMode) {

			this->imageLocation = imageStream->name;
			this->imageStream = imageStream;

			this->loadMode = loadMode;
		}
	};


	public ref class ImageModifiedEventArgs : public EventArgs
	{
	public:

		ImageModifiedEventArgs( bool isModified )
		{
			this->isModified = isModified;
		}

		bool isModified;

	};


	public ref class ImagePanel : public Panel
	{
	public:

		enum class CropStage {
			DISABLED,
			ENABLED,
			START_PRESSED,
			START_RELEASED
		};

	private:

		MediaFileFactory ^mediaFileFactory;
		MediaFile ^media;

		Bitmap ^sourceBitmap;
		PictureBox ^pictureBox;
		bool leftMouseButtonDown;
		Point startMouseLocation;
		Point scrollbarPosition;
		String ^_name;
		String ^_imageLocation;
		ImageFormat ^imageFormat;
		bool _isModified;
		bool _displayExceptions;
		bool _autoScaleImage;

		static const int THUMBNAIL_DATA = 0x501B;

		delegate void asyncLoadImageFinishedDelegate(AsyncLoadImageArgs ^args, ImageFormat ^format, Bitmap ^image);	
		delegate void displayMediaDelegate(MediaFile ^media);
		bool abortAsyncLoadImage;

		Point cropStart;
		CropStage _cropStage;
		Rectangle frame;


		void mouseMove(Object ^Sender, MouseEventArgs ^e) {

			if(leftMouseButtonDown && pictureBox->Image != nullptr) {

				Drawing::Size imageSize = getImageSize();

				float ratioY = imageSize.Height / float(Size.Height);
				float ratioX = imageSize.Width / float(Size.Width);

				Point imageLocation = Cursor::get()->Position;

				Point delta;
				delta.X = int((imageLocation.X - startMouseLocation.X) * -ratioX);
				delta.Y = int((imageLocation.Y - startMouseLocation.Y) * -ratioY);

				VScrollProperties ^vscroll = VerticalScroll::get();
				HScrollProperties ^hscroll = HorizontalScroll::get();

				if(vscroll->Visible == true) {

					int newPos = scrollbarPosition.Y + delta.Y;
					vscroll->Value = CLAMP(newPos, vscroll->Minimum, vscroll->Maximum);

				}

				if(hscroll->Visible == true) {

					int newPos = scrollbarPosition.X + delta.X;
					hscroll->Value = CLAMP(newPos, hscroll->Minimum, hscroll->Maximum);

				}

			}

			if(cropStage == CropStage::START_PRESSED || cropStage == CropStage::START_RELEASED) {

				if(frame.X != -1) {

					ControlPaint::DrawReversibleFrame( frame, Color::White, FrameStyle::Dashed );
				}

				Control ^sender = dynamic_cast<Control ^>(Sender);

				Point end = sender->PointToScreen(e->Location);

				frame = getCropRectangle(cropStart, end, false);

				ControlPaint::DrawReversibleFrame( frame, Color::White, FrameStyle::Dashed );

			}

		}

		void mouseDown(Object ^Sender, MouseEventArgs ^e) {

			if(dynamic_cast<PictureBox ^>(Sender)) {

				OnImageMouseDown(Sender, e);
			}

			if(e->Button == System::Windows::Forms::MouseButtons::Left) {

				Control ^sender = dynamic_cast<Control ^>(Sender);

				if(cropStage == CropStage::ENABLED) {

					cropStart = sender->PointToScreen(e->Location);	
					cropStage = CropStage::START_PRESSED;
					return;

				} else if(cropStage == CropStage::START_RELEASED) {

					cropImage(getCropRectangle(cropStart, sender->PointToScreen(e->Location), true));
					cropStage = CropStage::DISABLED;
					return;
				}

				leftMouseButtonDown = true;

				startMouseLocation = Cursor::get()->Position;

				VScrollProperties ^vscroll = VerticalScroll::get();
				scrollbarPosition.Y = vscroll->Value;

				HScrollProperties ^hscroll = HorizontalScroll::get();
				scrollbarPosition.X = hscroll->Value;

			}

			if(e->Button == System::Windows::Forms::MouseButtons::Right) {

				if(cropStage == CropStage::START_PRESSED || cropStage == CropStage::START_RELEASED) {

					cropStage = CropStage::ENABLED;

				} else if(cropStage == CropStage::ENABLED) {

					cropStage = CropStage::DISABLED;
				}
			}
		}


		void mouseUp(Object ^Sender, MouseEventArgs ^e) {

			if(e->Button == System::Windows::Forms::MouseButtons::Left) {

				leftMouseButtonDown = false;

				if(cropStage == CropStage::START_PRESSED) {

					cropStage = CropStage::START_RELEASED;
				}
			}
		}

		void mouseDoubleClick(Object ^Sender, MouseEventArgs ^e) {

			if(dynamic_cast<PictureBox ^>(Sender)) {

				OnImageMouseDoubleClick(Sender, e);
			}

		}


		void centerImage() {

			System::Drawing::Size maxDim = this->Size;
			System::Drawing::Size imageSize = getImageSize();		

			System::Drawing::Size autoScrollMinSize;

			if(imageSize.Width > maxDim.Width) {

				autoScrollMinSize.Width = imageSize.Width + 1;
			}

			if(imageSize.Height > maxDim.Height) {

				autoScrollMinSize.Height = imageSize.Height + 1;
			}

			AutoScrollMinSize = autoScrollMinSize;

			VScrollProperties ^vscroll = VerticalScroll::get();
			HScrollProperties ^hscroll = HorizontalScroll::get();

			AutoScroll = false;
			vscroll->Value = 0;
			hscroll->Value = 0;
			AutoScroll = true;

			int offsetX = 0;
			int offsetY = 0;

			if(imageSize.Width < maxDim.Width) {

				offsetX = (maxDim.Width - imageSize.Width) / 2;
			}

			if(imageSize.Height < maxDim.Height) {

				offsetY = (maxDim.Height - imageSize.Height) / 2;
			}

			pictureBox->Location = System::Drawing::Point(offsetX, offsetY);
		}


		void cropImage(Rectangle region) {

			if(sourceBitmap == nullptr || region.IsEmpty) return;

			Rectangle imageRect(0, 0, pictureBox->Image->Width, pictureBox->Image->Height);

			//if(!imageRect.Contains(region)) return;

			Bitmap ^croppedImage = gcnew Bitmap(region.Width, region.Height, sourceBitmap->PixelFormat);
			Graphics^ g = Graphics::FromImage(croppedImage);

			Rectangle dest(0, 0, region.Width, region.Height);

			g->DrawImage(pictureBox->Image, dest, region, GraphicsUnit::Pixel);

			sourceBitmap = (Bitmap ^)croppedImage->Clone();
			pictureBox->Image = croppedImage;

			centerImage();

			isModified = true;
		}

		Rectangle getCropRectangle(Point start, Point end, bool local) {

			Rectangle crop;

			start = pictureBox->PointToClient(start);
			end = pictureBox->PointToClient(end);

			if(end.X < start.X) {

				crop.X = end.X;
				crop.Width = start.X - end.X;

			} else {

				crop.X = start.X;
				crop.Width = end.X - start.X;
			}

			if(end.Y < start.Y) {

				crop.Y = end.Y;
				crop.Height = start.Y - end.Y;

			} else {

				crop.Y = start.Y;
				crop.Height = end.Y - start.Y;
			}

			Rectangle imageRect(0, 0, pictureBox->Image->Width, pictureBox->Image->Height);
			Rectangle empty(0,0,0,0);

			if(!imageRect.IntersectsWith(crop)) {

				return(empty);

			} else {

				crop.Intersect(imageRect);
			}

			if(!local) {

				crop.Location = pictureBox->PointToScreen(crop.Location);
			}

			return(crop);

		}

		

		//void asyncLoadImage(String ^fileLocation, bool loadMode) {
		void asyncLoadImage(Object ^state) {

			cli::array<Object ^> ^args = gcnew cli::array<Object ^>(3);
			Stream ^imageData = nullptr;

			Stream ^onlineImageData = nullptr;
			HttpWebResponse ^response = nullptr;

			try {

				// prevent new loads to happen before the previous load
				// has finished, the lock will automatically be released once
				// it goes out of scope
				// in the meantime try to speed the previous load by aborting it
				// Note: for this to work totally correctly it (faultly?) assumes lock is fifo
				abortAsyncLoadImage = true;
				msclr::lock l(this);
				abortAsyncLoadImage = false;

				AsyncLoadImageArgs ^imageArgs = dynamic_cast<AsyncLoadImageArgs ^>(state);

				ImageFormat ^imageFormat = nullptr;
				Bitmap ^image = nullptr;
				String ^fileName = Path::GetFileName(imageArgs->imageLocation);

				if(Util::isUrl(imageArgs->imageLocation) || imageArgs->imageStream != nullptr) {

					if(imageArgs->imageStream == nullptr) {

						HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(imageArgs->imageLocation);
						request->Method = L"GET";
						request->Timeout = 60 * 1000;

						IAsyncResult ^requestResult = request->BeginGetResponse(nullptr, nullptr);

						while(!requestResult->IsCompleted) {

							if(abortAsyncLoadImage == true) {

								request->Abort();
								//request->EndGetResponse(requestResult);
								return;
							}

							Thread::Sleep(100);

						}

						response = dynamic_cast<HttpWebResponse ^>(request->EndGetResponse(requestResult));
						onlineImageData = response->GetResponseStream();
					
						imageFormat = MediaFormatConvert::mimeTypeToImageFormat(response->ContentType);
					
						onlineImageData->ReadTimeout = 60 * 1000;	

					} else {

						onlineImageData = imageArgs->imageStream->data;
						imageFormat = MediaFormatConvert::mimeTypeToImageFormat(imageArgs->imageStream->mimeType);
					}									

					imageData = gcnew MemoryStream();

					int bufSize = 8096;
					int count = 0;

					cli::array<unsigned char> ^buffer = gcnew cli::array<unsigned char>(bufSize);

					while((count = onlineImageData->Read(buffer, 0, bufSize)) > 0) {

						if(abortAsyncLoadImage == true) {							
					
							return;
						}

						imageData->Write(buffer, 0, count);
					}


					imageData->Seek(0, System::IO::SeekOrigin::Begin);

				} else if(String::IsNullOrEmpty(imageArgs->imageLocation)) {

					args[0] = nullptr;

					this->Invoke(gcnew asyncLoadImageFinishedDelegate(this, &ImagePanel::asyncLoadImageFinished), args);
					return;

				} else {

					imageFormat = MediaFormatConvert::fileNameToImageFormat(imageArgs->imageLocation);
					imageData = File::OpenRead(imageArgs->imageLocation);
					//imageData = FileUtils::openAndLockFile(imageArgs->imageLocation, FileAccess::Read, FileShare::Read, 10, 500, false);

				}

				if(imageArgs->loadMode == AsyncLoadImageArgs::Mode::THUMBNAIL) {

					image = loadThumbNail(imageData);

				} else {

					Bitmap ^temp = gcnew Bitmap(imageData);

					image = gcnew Bitmap(temp->Width, temp->Height, temp->PixelFormat);

					Graphics^ g = Graphics::FromImage(image);
					g->DrawImage(temp, Rectangle(0,0,temp->Width, temp->Height));

					delete temp;
				}

				imageData->Close();

				// update the user interface,
				// Invoke is used and not beginInvoke to block until ui update is completed
				args[0] = imageArgs;
				args[1] = imageFormat;
				args[2] = image;			

				this->Invoke(gcnew asyncLoadImageFinishedDelegate(this, &ImagePanel::asyncLoadImageFinished), args);

			} catch(Exception ^e) {

				System::Diagnostics::Debug::WriteLine(e->Message);

				args[0] = nullptr;
				this->Invoke(gcnew asyncLoadImageFinishedDelegate(this, &ImagePanel::asyncLoadImageFinished), args);
					
			} finally {

				if(imageData != nullptr) {

					imageData->Close();
				}

				if(onlineImageData != nullptr) {

					onlineImageData->Close();
				}

				if(response != nullptr) {

					response->Close();
				}
			}

		}

		Bitmap ^loadThumbNail(Stream ^imageData) {

			Image ^image = Image::FromStream(imageData, false, false);

			// GDI+ throws an error if we try to read a property when the image
			// doesn't have that property. Check to make sure the thumbnail property
			// item exists.
			bool propertyFound = false;
			for(int i = 0; i < image->PropertyIdList->Length; i++) {

				if(image->PropertyIdList[i] == THUMBNAIL_DATA)
				{
					propertyFound = true;
					break;
				}

			}

			Image ^tempImage = nullptr;

			if(propertyFound) {

				PropertyItem ^p = image->GetPropertyItem(THUMBNAIL_DATA);

				imageData->Close();
				delete image;

				// The image data is in the form of a byte array. Write all 
				// the bytes to a stream and create a new image from that stream
				if(p->Value != nullptr) {
					
					cli::array<unsigned char> ^imageBytes = p->Value;
			
					MemoryStream ^stream = gcnew MemoryStream(imageBytes->Length);
					stream->Write(imageBytes, 0, imageBytes->Length);

					tempImage = Image::FromStream(stream);

				} else {

					tempImage = Image::FromStream(imageData);
				}

			} else {

				tempImage = Image::FromStream(imageData);
			}

			// scale thumbnail to the right size
			int thumbWidth;
			int thumbHeight;

			scaleToFitPanel(tempImage->Width, tempImage->Height, 0.9f, thumbWidth, thumbHeight);

			Bitmap ^thumbBitmap = nullptr;

			if(propertyFound) {

				// use the stored thumbnail
				thumbBitmap = gcnew Bitmap(tempImage, thumbWidth, thumbHeight);

			} else {

				// image does not contain a stored thumbnail, generate one instead
				Image::GetThumbnailImageAbort ^myCallback = gcnew Image::GetThumbnailImageAbort(this, &ImagePanel::thumbnailCallback);
				Image ^thumbImage = tempImage->GetThumbnailImage(thumbWidth, thumbHeight, myCallback, IntPtr::Zero);
				
				thumbBitmap = gcnew Bitmap(thumbImage);

			}

			if(tempImage != nullptr) {

				delete tempImage;
			}

			return(thumbBitmap);
		}

		// required useless callback function
		bool thumbnailCallback() {

			return(false);
		}

		void asyncLoadImageFinished(AsyncLoadImageArgs ^imageArgs, ImageFormat ^imageFormat, Bitmap ^image) {

			if(imageArgs != nullptr) {

				if(Util::isUrl(imageArgs->imageLocation)) {

					imageLocation = imageArgs->imageLocation;

				} else {

					imageLocation = Util::getProperFilePathCapitalization(imageArgs->imageLocation);
				}

				name = Path::GetFileName(imageLocation);

				sourceBitmap = image;

				isModified = false;

				if(imageArgs->loadMode == AsyncLoadImageArgs::Mode::SCALED) {

					int width, height;

					scaleToFitPanel(sourceBitmap->Width, sourceBitmap->Height, 1, width, height);
					pictureBox->Image = gcnew Bitmap(sourceBitmap, width, height);

					if(width != sourceBitmap->Width || height != sourceBitmap->Height) {

						isModified = true;
					}

				} else {

					pictureBox->Image = gcnew Bitmap(sourceBitmap);
					
				}

				this->imageFormat = imageFormat;

				centerImage();

				OnAsyncLoadImageFinished(this, EventArgs::Empty);

			} else {

				clearImage();
			}
		}

		String ^getThreadInfo() {

			int intAvailableThreads, intAvailableIoAsynThreds;

			ThreadPool::GetAvailableThreads(intAvailableThreads, intAvailableIoAsynThreds); 

			String ^pool = Thread::CurrentThread->IsThreadPoolThread == true ? "Pool" : "Non-Pool";
			String ^id = Convert::ToString(Thread::CurrentThread->GetHashCode());
			String ^free = Convert::ToString(intAvailableThreads);

			String ^strMessage = id + " (" + pool + ") Free " + free + " ";

			return(strMessage);
		}

		void scaleToFitPanel(int imageWidth, int imageHeight, float scaleFactor, int &scaledWidth, int &scaledHeight) {
			
			float widthScale = 1;
			float heightScale = 1;

			float maxWidth = Width * scaleFactor;
			float maxHeight = Height * scaleFactor;

			if(imageWidth > maxWidth) {
				
				widthScale = maxWidth / imageWidth;				
			}

			if(imageHeight > maxHeight) {
				
				heightScale = maxHeight / imageHeight;
				
			}

			scaledWidth = int(imageWidth * Math::Min(widthScale, heightScale));
			scaledHeight = int(imageHeight * Math::Min(widthScale, heightScale));
		}

		int getScaledWidth(int imageWidth, int imageHeight, int scaledHeight) {

			float heightScale = scaledHeight / float(imageHeight);
			int scaledWidth = CLAMP(int(imageWidth * heightScale), 1, MAX_IMAGE_SIZE);

			return(scaledWidth);

		}

		int getScaledHeight(int imageWidth, int imageHeight, int scaledWidth) {

			float widthScale = scaledWidth / float(imageWidth);
			int scaledHeight = CLAMP(int(imageHeight * widthScale), 1, MAX_IMAGE_SIZE);

			return(scaledHeight);
		}

	public:

		delegate void AsyncLoadImageFinished(System::Object ^sender, EventArgs ^e);
		event AsyncLoadImageFinished ^OnAsyncLoadImageFinished;

		delegate void ModifiedEventHandler(System::Object^ sender, ImageModifiedEventArgs^ e);
		event ModifiedEventHandler ^OnModified;

		delegate void ImageMouseDownEventHandler(System::Object^ sender, MouseEventArgs^ e);
		event ImageMouseDownEventHandler ^OnImageMouseDown;

		delegate void ImageMouseDoubleClickEventHandler(System::Object^ sender, MouseEventArgs^ e);
		event ImageMouseDoubleClickEventHandler ^OnImageMouseDoubleClick;

		property bool isModified {

		public: bool get() {

					return(_isModified);
				}

		protected: void set(bool isModified) {

					   _isModified = isModified;
					   OnModified(this, gcnew ImageModifiedEventArgs(isModified));

				   }
		}

		property bool isWebImage {

		public: bool get() {

					return(Util::isUrl(imageLocation));
				}
		}

		property bool isEmpty {

		public: bool get() {

					return(sourceBitmap == nullptr ? true : false);
				}
		}

		property String ^name {

		public: String ^get() {

					return(_name);
				}

		protected: void set(String ^name) {

					   _name = name;
				   }
		}

		property String ^imageLocation {

		public: String ^get() {

					return(_imageLocation);
				}

		protected: void set(String ^imageLocation) {

					   _imageLocation = imageLocation;
				   }
		}

		property CropStage cropStage {

			CropStage get() {

				return(_cropStage);
			}

			void set(CropStage cropStage) {

				_cropStage = cropStage;

				if(cropStage == CropStage::DISABLED) {

					Cursor = Cursors::Cross;
					frame.X = -1;

				} else if(cropStage == CropStage::ENABLED) {

					if(frame.X != -1) {

						ControlPaint::DrawReversibleFrame( frame, Color::White, FrameStyle::Dashed );
						frame.X = -1;
					}

					Cursor = Cursors::Hand;
				}
			}

		}

		property bool displayExceptions {

		public: bool get() {

					return(_displayExceptions);
				}

		public: void set(bool displayExceptions) {

					_displayExceptions = displayExceptions;
				}
		}

		ImagePanel()
		{		
			//AutoSize = true;
			Cursor = Cursors::Cross;
			AutoScroll = true;
			DoubleBuffered = true;

			this->pictureBox = gcnew PictureBox();
			this->pictureBox->SizeMode = PictureBoxSizeMode::AutoSize;

			Controls->Add(pictureBox);

			this->MouseMove += gcnew MouseEventHandler(this, &ImagePanel::mouseMove);
			this->MouseDown += gcnew MouseEventHandler(this, &ImagePanel::mouseDown);
			this->MouseUp += gcnew MouseEventHandler(this, &ImagePanel::mouseUp);

			pictureBox->MouseMove += gcnew MouseEventHandler(this, &ImagePanel::mouseMove);
			pictureBox->MouseDown += gcnew MouseEventHandler(this, &ImagePanel::mouseDown);
			pictureBox->MouseUp += gcnew MouseEventHandler(this, &ImagePanel::mouseUp);
			pictureBox->MouseDoubleClick += gcnew MouseEventHandler(this, &ImagePanel::mouseDoubleClick);

			leftMouseButtonDown = false;
			isModified = false;

			frame.X = -1;
			frame.Y = -1;

			displayExceptions = true;

			abortAsyncLoadImage = false;

			name = L"";
			imageLocation = L"";

			mediaFileFactory = gcnew MediaFileFactory();
			//mediaFileFactory->OpenFinished += gcnew EventHandler<MediaFile ^>(this, &ImagePanel::mediaFileFactory_OpenFinished);

		}

		System::Drawing::Size getImageSize(void) {

			if(pictureBox->Image == nullptr) return(System::Drawing::Size(0,0));

			System::Drawing::Size imageSize(pictureBox->Image->Width, pictureBox->Image->Height);

			return(imageSize);

		}

		void loadImage(String ^fileLocation, AsyncLoadImageArgs::Mode loadMode) {

			try {

				clearImage();

				WaitCallback ^asyncLoadImage = gcnew WaitCallback(this, &ImagePanel::asyncLoadImage);

				AsyncLoadImageArgs ^args = gcnew AsyncLoadImageArgs(fileLocation, loadMode);

				ThreadPool::QueueUserWorkItem(asyncLoadImage, args);
				
			} catch(Exception ^e) {

				MessageBox::Show(e->Message);
			}

		}

/*
		void loadImage(String ^fileLocation, AsyncLoadImageArgs::Mode loadMode) {

			mediaFile->open(fileLocation, loadMode);

		}
*/
		void mediaFileFactory_OpenFinished(System::Object ^sender, GEventArgs<MediaFile ^> ^e) {

			cli::array<Object ^> ^args = gcnew array<Object ^>(1);
			args[0] = e->Value;

			this->Invoke(gcnew displayMediaDelegate(this, &ImagePanel::displayMedia), args);		

		}

		void displayMedia(MediaFile ^media) {
			
			this->media = media;

			if(!media->OpenSuccess) return;
/*
			if(imageArgs->loadMode == AsyncLoadImageArgs::Mode::THUMBNAIL) {

					image = loadThumbNail(media->Data);

				} else {

					Bitmap ^temp = gcnew Bitmap(media->Data);

					image = gcnew Bitmap(temp->Width, temp->Height, temp->PixelFormat);

					Graphics^ g = Graphics::FromImage(image);
					g->DrawImage(temp, Rectangle(0,0,temp->Width, temp->Height));

					delete temp;
				}

				imageData->Close();

			media->Data->Close();
*/
		}


		void loadImage(ImageStream ^imageStream, AsyncLoadImageArgs::Mode loadMode) {

			try {

				clearImage();

				WaitCallback ^asyncLoadImage = gcnew WaitCallback(this, &ImagePanel::asyncLoadImage);

				AsyncLoadImageArgs ^args = gcnew AsyncLoadImageArgs(imageStream, loadMode);

				ThreadPool::QueueUserWorkItem(asyncLoadImage, args);
				
			} catch(Exception ^e) {

				MessageBox::Show(e->Message);
			}

		}

		void clearImage() {

			imageLocation = L"";
			name = L"";

			if(sourceBitmap != nullptr) {

				delete sourceBitmap;
				sourceBitmap = nullptr;
			}

			if(pictureBox->Image != nullptr) {

				delete pictureBox->Image;
				pictureBox->Image = nullptr;
			}

		}

		void resizeImage(int width, int height) {

			if(sourceBitmap == nullptr) return;

			System::Drawing::Size curSize = getImageSize();

			if(width == -2 && height == -2) {

				width = sourceBitmap->Width;
				height = sourceBitmap->Height;

			} else if(width == -1 && height == -1) {

				scaleToFitPanel(sourceBitmap->Width, sourceBitmap->Height, 1, width, height);

			} else if(width == -1) {

				width = getScaledWidth(curSize.Width, curSize.Height, height);

			} else if(height == -1) {

				height = getScaledHeight(curSize.Width, curSize.Height, width);
			}

			if(width != curSize.Width || height != curSize.Height) {

				pictureBox->Image = ImageUtils::resizeImage(sourceBitmap, width, height);
				isModified = true;
			}

			centerImage();			
		}

		void saveImage(String ^fileName) {

			pictureBox->Image->Save(fileName);
		}

		ImageStream ^saveImageToImageStream() {

			Stream ^stream = gcnew MemoryStream();

			pictureBox->Image->Save(stream, imageFormat);

			stream->Seek(0,System::IO::SeekOrigin::Begin);

			ImageStream ^imageStream = gcnew ImageStream(name, getMimeType(), stream);

			return(imageStream);
		}

		void rotateFlipImage(System::Drawing::RotateFlipType type) {

			Image^ temp = pictureBox->Image;

			sourceBitmap->RotateFlip(type);
			temp->RotateFlip(type);

			pictureBox->Image = temp;

			centerImage();

			isModified = true;
		}

		void zoomImage(float scale) {

			if(sourceBitmap == nullptr) return;

			Drawing::Size curSize = getImageSize();
			Drawing::Size newSize;

			newSize.Width = CLAMP(int(curSize.Width * scale), 1, MAX_IMAGE_SIZE);
			newSize.Height = CLAMP(int(curSize.Height * scale), 1, MAX_IMAGE_SIZE);

			resizeImage(newSize.Width, newSize.Height);

		}

		ImageFormat ^getImageFormat() {

			return(imageFormat);
		}

		String ^getMimeType() {

			return(MediaFormatConvert::imageFormatToMimeType(getImageFormat()));
		}

		PictureBox ^getPictureBox() {

			return(pictureBox);
		}


		/*
		virtual void OnSizeChanged(EventArgs ^e) override {

		Control::OnSizeChanged(e);

		centerImage();			
		}
		*/
		/*
		virtual void OnHandleCreated(EventArgs ^e) override {

		Control::OnHandleCreated(e);


		}
		*/
	};

}