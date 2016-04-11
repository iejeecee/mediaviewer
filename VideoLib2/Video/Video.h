#pragma once
#pragma warning(disable : 4244)
#include <algorithm>
#include "..\stdafx.h"
#include "VideoInit.h"
#include "Stream.h"
#include <msclr\marshal.h>

namespace VideoLib2 {


class Video {
	
protected:

	std::vector<VideoLib2::Stream *> stream;
	AVFormatContext *formatContext;
	
	int videoIdx;
	int audioIdx;
	
	static LOG_CALLBACK logCallback;

	Video() {

		formatContext = NULL;

		videoIdx = -1;
		audioIdx = -1;

		VideoInit::initializeAVLib();
			
	}

	

	virtual void addStream(VideoLib2::Stream *stream) {

		this->stream.push_back(stream);

		if(stream->getCodecContext()->codec_type == AVMEDIA_TYPE_VIDEO) {

			videoIdx = int(this->stream.size() - 1);

		} else if(stream->getCodecContext()->codec_type == AVMEDIA_TYPE_AUDIO) {

			audioIdx = int(this->stream.size() - 1);
		}
	}
	
public:


	enum FrameType {
		VIDEO,
		AUDIO
	};

	virtual ~Video() {
		
		close();
	}

	virtual void close() {
		
		if(formatContext != NULL) {
				
			for(unsigned int i = 0; i < stream.size(); i++) {
				
				delete stream[i];
			}
						
			stream.clear();		
		} 
	
		videoIdx = -1;
		audioIdx = -1;
	
	}

	virtual AVFormatContext *getFormatContext() const {

		return(formatContext);
	}

	virtual VideoLib2::Stream *getStream(int i) const 
	{		
		return(stream[i]);
	}

	virtual int getVideoStreamIndex() const {

		return(videoIdx);
	}

	virtual int getAudioStreamIndex() const {

		return(audioIdx);
	}

	virtual void setVideoStreamIndex(int idx) {

		videoIdx = idx;
	}

	virtual void setAudioStreamIndex(int idx) {

		audioIdx = idx;
	}

	virtual int getNrStreams() const {
		 
		return((int)stream.size());
	}

	virtual AVStream *getVideoStream() const {

		return(stream[videoIdx]->getAVStream());
	}

	virtual const AVCodec *getVideoCodec() const {

		return(stream[videoIdx]->getCodec());
	}

	virtual AVCodecContext *getVideoCodecContext() const {

		return(stream[videoIdx]->getCodecContext());
	}

	virtual AVStream *getAudioStream() const {

		return(stream[audioIdx]->getAVStream());
	}

	virtual const AVCodec *getAudioCodec() const {

		return(stream[audioIdx]->getCodec());
	}

	virtual AVCodecContext *getAudioCodecContext() const {

		return(stream[audioIdx]->getCodecContext());
	}

	
	virtual bool hasVideo() const {

		return(videoIdx >= 0);
	}

	virtual bool hasAudio() const {

		return(audioIdx >= 0);
	}
	

	
};



}