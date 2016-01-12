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
			throw gcnew VideoLib::VideoLibException("Error opening codec: ", result);
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
		
	AVStream *getAVStream() const {

		return(stream);
	}

	double getTimeSeconds(int64_t timeBaseUnits) 
	{		
		return av_q2d(stream->time_base) * timeBaseUnits;	
	}

	int64_t getTimeBaseUnits(double timeSeconds) 
	{
		int64_t startTime = stream->start_time == AV_NOPTS_VALUE ? 0 : stream->start_time;

		return int64_t(timeSeconds / av_q2d(stream->time_base));	
	}

	void listOptions() 
	{
		const AVOption *option = NULL;

		while((option = av_opt_next(codecContext,option)) != NULL) {

			String ^name = gcnew String(option->name);
			String ^help = gcnew String(option->help);

			System::Diagnostics::Debug::Print(name + ": " + help);
		}
	}
			
	void setOption(const std::string &name, const std::string &value) 
	{
		
		int result = av_opt_set(codecContext, name.c_str(), value.c_str(), 0);
		if(result != 0)
		{				
			throw gcnew VideoLib::VideoLibException("Error setting option: ", result);
		}

	}

	void listPrivateOptions() 
	{
		const AVOption *option = NULL;

		while((option = av_opt_next(codecContext->priv_data,option)) != NULL) {

			String ^name = gcnew String(option->name);
			String ^help = gcnew String(option->help);

			System::Diagnostics::Debug::Print(name + ": " + help);
		}
	}

	void setPrivateOption(const std::string &name, const std::string &value) 
	{
		int result = av_opt_set(codecContext->priv_data, name.c_str(), value.c_str(), 0);
		if(result != 0)
		{	
			throw gcnew VideoLib::VideoLibException("Error setting private option: ", result);
		}
	}
};




}
