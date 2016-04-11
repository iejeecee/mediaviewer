#pragma once
#pragma warning(disable : 4244)
// unsafe function warning disable
#pragma warning(disable : 4996)
#include <algorithm>
#include "..\stdafx.h"
#include "VideoInit.h"
#include "..\VideoLibException.h"
#include <msclr\marshal.h>


namespace VideoLib2 {

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
			throw gcnew VideoLib2::VideoLibException("Error opening codec: ", result);
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

	bool isVideo() const {

		return(codecContext->codec_type == AVMEDIA_TYPE_VIDEO);
	}

	bool isAudio() const {

		return(codecContext->codec_type == AVMEDIA_TYPE_AUDIO);
	}

	AVMediaType getCodecType() const {

		return(codecContext->codec_type);
	}

	AVCodecID getCodecID() const {

		return(codecContext->codec_id);
	}

	const AVCodecDescriptor *getCodecDescriptor() const {

		return av_codec_get_codec_descriptor(getCodecContext());
	}
		
	AVStream *getAVStream() const {

		return(stream);
	}

	double getTimeSeconds(int64_t timeBaseUnits) const
	{		
		return av_q2d(stream->time_base) * timeBaseUnits;	
	}

	int64_t getTimeBaseUnits(double timeSeconds) const
	{
		
		return int64_t(timeSeconds / av_q2d(stream->time_base));	
	}

	int64_t getStartTime() const
	{
		return stream->start_time == AV_NOPTS_VALUE ? 0 : stream->start_time;
	}

	// return stream timebase
	AVRational getTimeBase() const
	{
		return stream->time_base;
	}
			
	void setOption(const std::string &name, const std::string &value) 
	{
		
		int result = av_opt_set(codecContext, name.c_str(), value.c_str(), AV_OPT_SEARCH_CHILDREN);
		if(result != 0)
		{				
			throw gcnew VideoLib2::VideoLibException("Error setting option: " + gcnew String(name.c_str()) + " ", result);
		}

	}

	
};




}
