#pragma once

namespace imageviewer {

using namespace VideoLib;

public ref class VideoLibTest 
{

private:


public: 

	VideoLibTest()
	{
		
	}

	void start() {

		String ^testFile = "";

		VideoLib::VideoPreview ^videoPreview = gcnew VideoPreview();

		for(int i = 0; i < 200; i++) {
			
			//List<VideoLib::RawImageRGB24 ^> ^images = videoPreview->grabThumbnails(testFile, 160, 160, -1, 1);

			//delete images[0];
		}

	}

};



}