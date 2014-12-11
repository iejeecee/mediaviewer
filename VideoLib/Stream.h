#pragma once
#pragma warning(disable : 4244)
// unsafe function warning disable
#pragma warning(disable : 4996)
#include <algorithm>
#include "stdafx.h"
#include "VideoInit.h"
#include "VideoLibException.h"
#include <msclr\marshal.h>


namespace VideoLib {

class Stream {

protected:

	AVCodecContext *codecContext;
	const AVCodec *codec;
	
	AVStream *stream;
	
public:

	Stream(AVStream *stream, const AVCodec *codec) {
		
		this->stream = stream;
		this->codecContext = stream->codec;
		this->codec = codec;
	}
			
	~Stream() {

		close();
		
	}

	
	void open() {
				
		int result = avcodec_open2(codecContext, codec, NULL);
		if(result != 0)
		{	
			throw gcnew VideoLib::VideoLibException("Error opening codec: " + VideoInit::errorToString(result));
		}

	}
				
	virtual void close() {

		if(codecContext != NULL) {

			avcodec_close(codecContext);		
			codecContext = NULL;
		}
					
	}

	const AVCodec *getCodec() const {

		return(codec);
	}

	AVCodecContext *getCodecContext() const {

		return(codecContext);
	}
		
	AVStream *getStream() const {

		return(stream);
	}
			
	
		
};




}
