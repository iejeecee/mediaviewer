#pragma once
#include "stdafx.h"
#include "Video.h"
#include "..\VideoLibException.h"
#include "..\FilterGraph\FilterGraph.h"
#include "IVideoDecoder.h"
#include "..\MediaTransformer\MemoryStreamAVIOContext.h"
#include <algorithm>
#include <msclr\marshal_cppstd.h>

using namespace System::Threading;
using namespace msclr::interop;
using namespace System::Runtime::InteropServices;

namespace VideoLib2 {

typedef void (__stdcall *DECODED_FRAME_CALLBACK)(void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type);

class VideoDecoder : public IVideoDecoder {

protected:

	AVFrame *frame;
	
	SwsContext *imageConvertContext;
	SwrContext *audioConvertContext;

	AVDictionary *openOptions;

	// video
	AVPixelFormat inPixelFormat, outPixelFormat;
	int inWidth, outWidth;
	int	inHeight, outHeight;

	// audio
	int inSampleRate, outSampleRate;
	int64_t inChannelLayout, outChannelLayout;
	AVSampleFormat inSampleFormat, outSampleFormat;

	int outNrChannels;
	int outBytesPerSample;
	
	AVFrame *convertedFrame;
	uint8_t *convertedFrameBuffer;

	double startTime;

	void *data;

	DECODED_FRAME_CALLBACK decodedFrame;

	bool closed;

	AVPacket packet;

	// video
	int64_t sizeBytes;
	
	// audio
	int samplesPerSecond;
	int bytesPerSample;
	int nrChannels;

	double durationSeconds;

	FilterGraph *videoFilterGraph;
	FilterGraph *audioFilterGraph;

	static void logCallback(int level, const char *message) {
		
		Console::WriteLine(gcnew System::String(message)); 
	}

	double calcDurationSeconds() {

		double duration = (double)formatContext->duration / AV_TIME_BASE;
		
		if(duration < 0) {

			if(hasVideo()) {
							
				duration = getVideoStream()->duration * av_q2d(getVideoStream()->time_base);
			} 
			else if(hasAudio()) {

				duration = getAudioStream()->duration * av_q2d(getAudioStream()->time_base);
			}
		}
		
		if(duration < 0) {
										
			duration = 0;
		}

		return(duration);
	}

	

	gcroot<CancellationToken ^> *cancellationToken;

	
private:

	SamplingMode samplingMode;

	AVIOInterruptCB ioInterruptSettings;
		
	static int ioInterruptCallback(void *token) 
	{ 
		gcroot<CancellationToken^> &cancellationToken = *((gcroot<CancellationToken^>*)token);
		
		if(cancellationToken->IsCancellationRequested) {

			return(1);

		} else {	

			return 0;
		}
	} 

	void open(const char *locationUTF8, System::Threading::CancellationToken ^token, AVInputFormat *inputFormat = NULL) {

		int errorCode = 0;

		// Initialize Input format context
		if((errorCode = avformat_open_input(&formatContext, locationUTF8, inputFormat, &openOptions)) != 0)
		{		
			if(errorCode == AVERROR_EXIT || errorCode == AVERROR_EOF) {
				
				throw gcnew System::OperationCanceledException(*token);

			} else {

				throw gcnew VideoLib2::VideoLibException("Unable to open the stream: ", errorCode);
			}
		}
	
		// generate pts?? -- from ffplay, not documented
		// it should make av_read_frame() generate pts for unknown value
		formatContext->flags |= AVFMT_FLAG_GENPTS;
		
		if((errorCode = avformat_find_stream_info(formatContext, NULL)) < 0)
		{		
			if(errorCode == AVERROR_EXIT || errorCode == AVERROR_EOF) {

				throw gcnew System::OperationCanceledException(*token);

			} else {

				throw gcnew VideoLib2::VideoLibException("Unable to find the stream's info: ", errorCode);
			}
		}
		
		for(unsigned int i = 0; i < formatContext->nb_streams; i++) {

			AVCodec *decoder = NULL;

			if(formatContext->streams[i]->codec->codec_type == AVMediaType::AVMEDIA_TYPE_VIDEO ||
				formatContext->streams[i]->codec->codec_type == AVMediaType::AVMEDIA_TYPE_AUDIO)
			{
				decoder = avcodec_find_decoder(formatContext->streams[i]->codec->codec_id);
				if(decoder == NULL)
				{		
					VideoInit::writeToLog(AV_LOG_ERROR, "Decoder not found for input stream");
					//throw gcnew VideoLib2::VideoLibException("Decoder not found for input stream");
				}
			}
			
			VideoLib2::Stream *newStream = new VideoLib2::Stream(formatContext->streams[i], decoder);			
		
			stream.push_back(newStream);			
		}

		videoIdx = av_find_best_stream(formatContext, AVMEDIA_TYPE_VIDEO, -1, -1, NULL, 0);
		if(videoIdx >= 0) {
					
			if(stream[videoIdx]->getCodec() != NULL)
			{
				stream[videoIdx]->open();

			} else {

				// no decoder for video
				videoIdx = 0;
			}
			
		}
			
		audioIdx = av_find_best_stream(formatContext, AVMEDIA_TYPE_AUDIO, -1, -1, NULL, 0);
		if(audioIdx >= 0) {
					
			if(stream[audioIdx]->getCodec() != NULL)
			{
				stream[audioIdx]->open();

			} else {

				// no decoder for audio
				audioIdx = 0;
			}
			
		}
			
		frame = av_frame_alloc();
		if(frame == NULL)
		{		
			throw gcnew VideoLib2::VideoLibException("Unable to allocate frame memory");
		}

		if(formatContext->start_time == AV_NOPTS_VALUE) {

			startTime = 0;

		} else {

			startTime = formatContext->start_time / AV_TIME_BASE;
		}

		durationSeconds = calcDurationSeconds();
						
		av_init_packet(&packet);
		packet.data = NULL;
		packet.size = 0;

		// int64_t duration_tb = duration / av_q2d(pStream->time_base); 
		closed = false;

		sizeBytes = formatContext->pb ? avio_size(formatContext->pb) : 0;

		if(hasAudio()) {
			
			if(getAudioCodecContext()->channel_layout == 0) {

				getAudioCodecContext()->channel_layout = av_get_default_channel_layout(getAudioCodecContext()->channels);
			}

			samplesPerSecond = getAudioCodecContext()->sample_rate;
			bytesPerSample = av_get_bytes_per_sample(getAudioCodecContext()->sample_fmt);
			
			nrChannels = getAudioCodecContext()->channels;
		
		} 

		// make sure a cancel exception is thrown if open was cancelled at any point during it's execution
		token->ThrowIfCancellationRequested();
	}

	bool isVideo() const {
	
		if(!hasVideo()) return false;	

		if((getVideoStream()->disposition & AV_DISPOSITION_ATTACHED_PIC) > 0) return false; //mp3 cover art etc

		std::string container = std::string(formatContext->iformat->long_name);

		if(container.compare("image2 sequence") == 0) return(false); //jpeg

		bool hasFramerate = getFrameRate() > 0;

		bool hasDuration = getDurationSeconds() > 0;

		return hasFramerate || hasDuration;
	}

	bool isAudio() const {

		if(!hasAudio()) return false;		
		if(getFrameRate() > 0) return false;
		
		if(hasVideo()) {
		
			if((getVideoStream()->disposition & AV_DISPOSITION_ATTACHED_PIC) == 0) return false;
		}

		return true;	
	}

	bool isImage() const {

		if(hasAudio()) return false;
		if(getFrameRate() > 0) return false;

		std::string container = std::string(formatContext->iformat->long_name);

		if(container.compare("image2 sequence") == 0) return(true); //jpeg
		if(getDurationSeconds() > 0) return false;
		if(!hasVideo()) return false;
	
		return(true);

	}

public:

	enum VideoDecodeMode {		
		DECODE_KEY_FRAMES_ONLY,
		DECODE_VIDEO,
		SKIP_VIDEO
	};

	enum AudioDecodeMode {
		DECODE_AUDIO,
		SKIP_AUDIO
	};
	
	VideoDecoder()
	{
		openOptions = NULL;
		
		decodedFrame = NULL;
		data = NULL;

		frame = NULL;

		imageConvertContext = NULL;
		audioConvertContext = NULL;
		convertedFrame = NULL;
		convertedFrameBuffer = NULL;

		startTime = 0;
			
		closed = true;

		samplesPerSecond = 0;
		bytesPerSample = 0;
		nrChannels = 0;
		sizeBytes = 0;

		durationSeconds = 0;

		outPixelFormat = inPixelFormat = AV_PIX_FMT_YUV420P;
		inWidth = outWidth = 0;
		inHeight = outHeight = 0;

		outSampleFormat = inSampleFormat = AV_SAMPLE_FMT_S16;
		outNrChannels = 0;
		outSampleRate = inSampleRate = 0;
		outChannelLayout = inChannelLayout = 0;
		outBytesPerSample = 0;

		samplingMode = X;

		videoFilterGraph = NULL;
		audioFilterGraph = NULL;	
		
		cancellationToken = new gcroot<CancellationToken ^>();
	}

	virtual ~VideoDecoder() {
		
		close();
	
		delete cancellationToken;
	}

	bool isClosed() const {

		return(closed);
	}

	void setDecodedFrameCallback(DECODED_FRAME_CALLBACK decodedFrame = NULL,
		void *data = NULL) 
	{
		this->decodedFrame = decodedFrame;
		this->data = data;
	}

	virtual void close() {
		
		Video::close();

		if(formatContext != NULL) {

			avformat_close_input(&formatContext);
			formatContext = NULL;
		}

		if(frame != NULL) {

			av_free(frame);
			frame = NULL;
		}

		if(convertedFrame != NULL) {

			av_free(convertedFrame);
			convertedFrame = NULL;
		}

		if(convertedFrameBuffer != NULL) {

			av_free(convertedFrameBuffer);
			convertedFrameBuffer = NULL;
		}

		if(imageConvertContext != NULL) {

			sws_freeContext(imageConvertContext);
			imageConvertContext = NULL;
		}

		if(audioConvertContext != NULL) {

			swr_free(&audioConvertContext);
			audioConvertContext = NULL;
		}

		if(videoFilterGraph != NULL) {

			delete videoFilterGraph;
			videoFilterGraph = NULL;
		}

		if(audioFilterGraph != NULL) {

			delete audioFilterGraph;
			audioFilterGraph = NULL;
		}

		if(openOptions != NULL) {

			av_dict_free(&openOptions);
		}

		outPixelFormat = inPixelFormat = AV_PIX_FMT_YUV420P;
		inWidth = outWidth = 0;
		inHeight = outHeight = 0;

		outSampleFormat = inSampleFormat = AV_SAMPLE_FMT_S16;
		outNrChannels = 0;
		outSampleRate = inSampleRate = 0;
		outChannelLayout = inChannelLayout = 0;
		outBytesPerSample = 0;	

		samplingMode = X;

		startTime = 0;
			
		closed = true;
	}

	std::string getFileName() const {

		return(formatContext->filename);
	}

	void open(MemoryStreamAVIOContext *memoryStreamCtx, System::Threading::CancellationToken ^token,
		const char *formatName = NULL) 
	{
		// allow cancellation of IO operations
		*cancellationToken = token;
		
		ioInterruptSettings.callback = ioInterruptCallback;
		ioInterruptSettings.opaque = cancellationToken;

		formatContext = avformat_alloc_context();		
		formatContext->interrupt_callback = ioInterruptSettings;

		// set custom aviocontext
		formatContext->flags |= AVFMT_FLAG_CUSTOM_IO;
		formatContext->pb = memoryStreamCtx->getAVIOContext();

		AVInputFormat *inputFormat = NULL;

		if(formatName != NULL) {

			inputFormat = av_find_input_format(formatName);
		}

		open(NULL, token, inputFormat);
	}
	
	virtual void open(OpenVideoArgs ^args, System::Threading::CancellationToken ^token) 
	{

		String ^location = args->VideoLocation != nullptr ? args->VideoLocation : args->AudioLocation;

		// Convert location to UTF8 string pointer
		array<Byte>^ encodedBytes = System::Text::Encoding::UTF8->GetBytes(location);

		// prevent GC moving the bytes around while this variable is on the stack
		pin_ptr<Byte> pinnedBytes = &encodedBytes[0];

		// Call the function, typecast from byte* -> char* is required
		char *locationUTF8 = reinterpret_cast<char*>(pinnedBytes);
				
		// allow cancellation of IO operations
		*cancellationToken = token;
		
		ioInterruptSettings.callback = ioInterruptCallback;
		ioInterruptSettings.opaque = cancellationToken;
 		
		formatContext = avformat_alloc_context();
		formatContext->interrupt_callback = ioInterruptSettings;
			
		// potentially speed up opening video if we know it's type beforehand
		AVInputFormat *inputFormat = NULL;
				
		String ^inputType = args->VideoType != nullptr ? args->VideoType : nullptr;
		inputType = args->AudioType != nullptr ? args->AudioType : nullptr;

		if(inputType != nullptr) {

			IntPtr p = Marshal::StringToHGlobalAnsi(inputType);
			char *shortName = static_cast<char*>(p.ToPointer());

			inputFormat = av_find_input_format(shortName);
						
			//av_dict_set(&openOptions, "formatprobesize", "0", 0); 
			//av_dict_set(&openOptions, "analyzeduration", "32", 0); 
			//av_dict_set(&openOptions, "probesize", "32", 0); 

			//formatContext->flags |= AVFMT_FLAG_IGNIDX;		

			Marshal::FreeHGlobal(p);			
		}

		
		open(locationUTF8, token, inputFormat);	    
	}

	int64_t getSizeBytes() const {

		return(sizeBytes);
	}

	// returns 0 if unknown or no video 
	double getFrameRate() const {

		double frameRate = 0;

		if(!hasVideo()) return frameRate;
		
		if(getVideoStream()->avg_frame_rate.den != 0 && getVideoStream()->avg_frame_rate.num != 0) 
		{
			frameRate = av_q2d(getVideoStream()->avg_frame_rate);	

		} else {

			frameRate = av_q2d(getVideoCodecContext()->framerate);	
		}
		
		return frameRate;
	}

	int getAudioSamplesPerSecond() const {

		return(samplesPerSecond);
	}

	int getAudioBytesPerSample() const  {

		return(bytesPerSample);
	}

	int getAudioNrChannels() const {

		return(nrChannels);
	}

	int getAudioBytesPerSecond() const {

		return(samplesPerSecond * bytesPerSample * nrChannels);
	}

	double getDurationSeconds() const {
		
		return(durationSeconds);
	}

	AVPixelFormat getOutputPixelFormat() const {

		return(outPixelFormat);
	}

	AVSampleFormat getOutputSampleFormat() const {

		return(outSampleFormat);
	}

	int getOutputSampleRate() const {

		return(outSampleRate);
	}

	int64_t getOutputChannelLayout() const {

		return(outChannelLayout);
	}

	int getOutputNrChannels() const {

		return(outNrChannels);
	}

	int getOutputBytesPerSample() const {

		return(outBytesPerSample);
	}

	FilterGraph *getVideoFilterGraph() const {

		return(videoFilterGraph);
	}

	FilterGraph *getAudioFilterGraph() const {

		return(audioFilterGraph);
	}
	
	// reads a frame from the input stream and places it into a packet
	ReadFrameResult readFrame(AVPacket *packet) {
		
		if((*cancellationToken)->IsCancellationRequested) {

			return(ReadFrameResult::END_OF_STREAM);
		}

		int error = av_read_frame(getFormatContext(), packet);
		if(error < 0) 
		{					
			if(error == AVERROR_INVALIDDATA)
			{
				VideoInit::writeToLog(AV_LOG_ERROR, "Error reading frame from input stream");	
				return(ReadFrameResult::READ_ERROR);

			} else {	

				return(ReadFrameResult::END_OF_STREAM);
			}
		
		} else {
			
			return(ReadFrameResult::OK);
		}
	}

	int decodeVideoFrame(AVFrame *picture, int *got_picture_ptr, const AVPacket *avpkt) {

		int ret = avcodec_decode_video2(getVideoCodecContext(), 
					picture, got_picture_ptr, avpkt);
		if(ret < 0) {

			VideoInit::writeToLog(AV_LOG_WARNING, "could not decode video frame");		
		}

		return ret;
	}

	// returns number of bytes decoded
	int decodeAudioFrame(AVFrame *audio, int *got_audio_ptr, const AVPacket *avpkt) {

		int ret = avcodec_decode_audio4(getAudioCodecContext(), 
						audio, got_audio_ptr, avpkt);	
		if(ret < 0) {

			VideoInit::writeToLog(AV_LOG_WARNING, "could not decode audio frame");				
		}

		return ret;
	}	

	bool seek(double posSeconds, int flags = AVSEEK_FLAG_BACKWARD) {
						
		/*int streamIndex;

		switch(getMediaType()) 
		{
		case MediaType::VIDEO_MEDIA:
			{
				streamIndex = getVideoStreamIndex();
				break;
			}
		case MediaType::AUDIO_MEDIA:
			{
				streamIndex = getAudioStreamIndex();
				break;
			}
		default:
			{
				VideoInit::writeToLog(AV_LOG_WARNING, "cannot seek in non-video/audio media");	
				return(false);
			}
		}
		
		int64_t seekTarget = getStream(streamIndex)->getTimeBaseUnits(posSeconds) + getStream(streamIndex)->getStartTime();*/

		//int ret = av_seek_frame(formatContext, streamIndex, position, flags);

		// convert timestamp into a videostream timestamp
		AVRational myAVTIMEBASEQ = {1, AV_TIME_BASE}; 
			
		int64_t seekTarget = posSeconds / av_q2d(myAVTIMEBASEQ);

		int64_t min_ts, max_ts;

		if(flags & AVSEEK_FLAG_BACKWARD) 
		{
			min_ts = INT64_MIN;
			max_ts = seekTarget;

		} else {

			min_ts = seekTarget;
			max_ts = INT64_MAX;
		}

		int ret = avformat_seek_file(formatContext, -1, min_ts, seekTarget, max_ts, flags);
		if(ret >= 0) { 
			
			if(hasVideo()) {

				avcodec_flush_buffers(getVideoCodecContext());
			}

			if(hasAudio()) {

				avcodec_flush_buffers(getAudioCodecContext());
			}
			
			return true;
		}

		return false;
		
	}

	
	int getWidth() const {

		return(hasVideo() ? getVideoCodecContext()->width : 0);
	}

	int getHeight() const {

		return(hasVideo() ? getVideoCodecContext()->height : 0);
	}

	void setVideoOutputFormat(AVPixelFormat pixelFormat, int width, int height, 
		SamplingMode sampling) 
	{
		outPixelFormat = pixelFormat;
		outWidth = width;
		outHeight = height;
		samplingMode = sampling;
		
		if(convertedFrame != NULL) {

			av_frame_free(&convertedFrame);
			convertedFrame = NULL;
		}

		convertedFrame = av_frame_alloc();
		if(convertedFrame == NULL)
		{
			throw gcnew VideoLib2::VideoLibException("Unable to allocate frame memory");
		}

		int numBytes = avpicture_get_size(outPixelFormat, outWidth, outHeight);
		convertedFrameBuffer = (uint8_t*)av_malloc(numBytes);

		avpicture_fill((AVPicture*)convertedFrame, convertedFrameBuffer, outPixelFormat, outWidth, outHeight);

	}

	void convertVideoFrame(AVFrame *input, AVFrame *output)
	{				
		if(input->format == -1) {
			//incorrect frame
			return;
		}

		imageConvertContext = sws_getCachedContext(imageConvertContext, input->width, input->height, (AVPixelFormat)input->format,
			outWidth, outHeight, outPixelFormat, samplingMode, NULL, NULL, NULL);

		if(imageConvertContext == NULL) {

			throw gcnew VideoLib2::VideoLibException("Unable to allocate video convert context");
		} 
		
		av_frame_copy_props(output, input);

		// convert frame to the right format
		sws_scale(imageConvertContext,
			input->data,
			input->linesize,
			0,
			input->height,
			output->data,
			output->linesize);	

		output->width = outWidth;
		output->height = outHeight;
		output->format = outPixelFormat;

		output->pts = av_frame_get_best_effort_timestamp(input);
			
	}
	
	void setAudioOutputFormat(int sampleRate = 44100, int64_t channelLayout = AV_CH_LAYOUT_STEREO, 
		AVSampleFormat sampleFormat = AV_SAMPLE_FMT_S16)
	{
		outSampleFormat = sampleFormat;
		outChannelLayout = channelLayout;
		outSampleRate = sampleRate;
		outNrChannels = av_get_channel_layout_nb_channels(channelLayout);
		outBytesPerSample = av_get_bytes_per_sample(sampleFormat);
	}

	int convertAudioFrame(AVFrame *input, AVFrame *output) 
	{
		if(input->format == -1) {
			//incorrect frame
			return(0);
		}

		if(input->channel_layout != inChannelLayout ||
			input->sample_rate != inSampleRate ||
			input->format != inSampleFormat) 
		{
			if(audioConvertContext != NULL) {

				swr_free(&audioConvertContext);
				audioConvertContext = NULL;
			}
				
			inChannelLayout = input->channel_layout;
			inSampleRate = input->sample_rate;
			inSampleFormat = (AVSampleFormat)input->format;	
		}

		if(audioConvertContext == NULL) {

			audioConvertContext = swr_alloc_set_opts(NULL,
				outChannelLayout,
				outSampleFormat,
				outSampleRate,
				inChannelLayout,
				inSampleFormat,
				inSampleRate,
				0,
				NULL);

			if(audioConvertContext == NULL) {

				throw gcnew VideoLib2::VideoLibException("Unable to allocate audio convert context");
			}

			if(swr_init(audioConvertContext) != 0)
			{
				throw gcnew VideoLib2::VideoLibException("Unable to initialize audio convert context");
			}

		}
	
		av_frame_copy_props(output, input);

		int numSamplesOut = swr_convert(audioConvertContext,
										output->data,
										input->nb_samples,
										(const unsigned char**)input->extended_data,
										input->nb_samples);
		// audio length does not equal linesize, because some extra
		// padding bytes may be added for alignment.
		// Instead av_samples_get_buffer_size needs to be used
		int length = av_samples_get_buffer_size(NULL,
											outNrChannels,
											numSamplesOut,
											outSampleFormat, 1);									
				
		output->sample_rate = outSampleRate;
		output->channel_layout = outChannelLayout;
		output->format = outSampleFormat;

		output->pts = av_frame_get_best_effort_timestamp(input);

		return(length);
	}

	MediaType getMediaType() const 
	{	
		if(isVideo()) return MediaType::VIDEO_MEDIA;
		if(isAudio()) return MediaType::AUDIO_MEDIA;
		if(isImage()) return MediaType::IMAGE_MEDIA;
		
		return MediaType::UNKNOWN_MEDIA;
	}

	SeekMode getSeekMode() const 
	{
		if((getFormatContext()->iformat->flags & AVFMT_SEEK_TO_PTS) > 0) {

			return SeekMode::SEEK_BY_PTS;

		} else {

			return SeekMode::SEEK_BY_DTS;
		}

	}



	
	
};

}
