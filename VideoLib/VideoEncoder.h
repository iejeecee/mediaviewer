#pragma once
#include "stdafx.h"
#include "Video.h"
#include "VideoLibException.h"

namespace VideoLib {

class VideoEncoder : public Video {

protected:

	AVOutputFormat *outFormat;

public: 
	
	VideoEncoder() {

		outFormat = NULL;
		
	}

	virtual void open(String ^outputFilename) {

		// Convert location to UTF8 string pointer
		array<Byte>^ encodedBytes = System::Text::Encoding::UTF8->GetBytes(outputFilename);

		// prevent GC moving the bytes around while this variable is on the stack
		pin_ptr<Byte> pinnedBytes = &encodedBytes[0];

		// Call the function, typecast from byte* -> char* is required
		char *outputUTF8 = reinterpret_cast<char*>(pinnedBytes);
			
		avformat_alloc_output_context2(&formatContext, NULL, NULL, outputUTF8);
		
		if(formatContext == NULL) {

			throw gcnew VideoLib::VideoLibException("Cannot create output format context: " + outputFilename);
		}
	
		outFormat = formatContext->oformat;
				
	}

	virtual void close() {

		if(formatContext != NULL) {
					
			for (unsigned int i = 0; i < formatContext->nb_streams; i++) {

				avcodec_close(formatContext->streams[i]->codec);			
			}			
		} 

		 if (formatContext != NULL && !(formatContext->oformat->flags & AVFMT_NOFILE)) 
		 {
			avio_close(formatContext->pb);		

			avformat_free_context(formatContext);

			formatContext = NULL;
		 }

		 Video::close();
	}

	AVOutputFormat *getOutputFormat() const {

		return(outFormat);
	}
};

}