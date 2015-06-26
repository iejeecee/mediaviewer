#pragma once
#include "stdafx.h"
#include "Video.h"
#include "VideoLibException.h"
#include "FilterGraph.h"
#include <algorithm>

using namespace System::Threading;

namespace VideoLib {

typedef void (__stdcall *DECODED_FRAME_CALLBACK)(void *data, AVPacket *packet, AVFrame *frame, Video::FrameType type);

class VideoDecoder : public Video {

protected:

	AVFrame *frame;
	
	SwsContext *imageConvertContext;
	SwrContext *audioConvertContext;

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

	int durationSeconds;

	FilterGraph *videoFilterGraph;
	FilterGraph *audioFilterGraph;

	static void logCallback(int level, const char *message) {
		
		Console::WriteLine(gcnew System::String(message)); 
	}

	int calcDurationSeconds() {

		int duration = formatContext->duration / AV_TIME_BASE;
		
		if(duration < 0) {

			if(hasVideo()) {

				if(strcmp(stream[videoIdx]->getCodec()->name, "gif")) {

					// should calculate animated gif duration in some other fashion?
					return(0);
				}

				duration = getVideoStream()->duration * av_q2d(getVideoStream()->time_base);
			} 
			else if(hasAudio()) {

				duration = getAudioStream()->duration * av_q2d(getAudioStream()->time_base);
			}
		}
		
		if(duration < 0) {
					
			throw gcnew VideoLib::VideoLibException("can't determine video duration");
		}

		return(duration);
	}

	gcroot<CancellationToken ^> *cancellationToken;

public:

	enum SamplingMode {
		FAST_BILINEAR = SWS_FAST_BILINEAR,
		BILINEAR = SWS_BILINEAR,
		BICUBIC = SWS_BICUBIC,
		X = SWS_X,
		POINT = SWS_POINT,
		AREA = SWS_AREA,
		BICUBLIN = SWS_BICUBLIN,
		GAUSS = SWS_GAUSS,
		SINC = SWS_SINC,
		LANCZOS = SWS_LANCZOS,
		SPLINE = SWS_SPLINE
	};

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

	bool isClosed() {

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
	
	virtual void open(String ^location, System::Threading::CancellationToken ^token, String ^inputFormatName = nullptr) {

		// Convert location to UTF8 string pointer
		array<Byte>^ encodedBytes = System::Text::Encoding::UTF8->GetBytes(location);

		// prevent GC moving the bytes around while this variable is on the stack
		pin_ptr<Byte> pinnedBytes = &encodedBytes[0];

		// Call the function, typecast from byte* -> char* is required
		char *locationUTF8 = reinterpret_cast<char*>(pinnedBytes);

		int errorCode = 0;

		AVInputFormat *inputFormat = NULL;
		 				
		if(inputFormat != nullptr) {

			IntPtr p = Marshal::StringToHGlobalAnsi(inputFormatName);
			char *shortName = static_cast<char*>(p.ToPointer());

			inputFormat = av_find_input_format(shortName);

			Marshal::FreeHGlobal(p);			
		}

		*cancellationToken = token;
		
		ioInterruptSettings.callback = ioInterruptCallback;
		ioInterruptSettings.opaque = cancellationToken;
 
		 // Initialize Input format context
		formatContext = avformat_alloc_context();
		formatContext->interrupt_callback = ioInterruptSettings;
		
		if((errorCode = avformat_open_input(&formatContext, locationUTF8, inputFormat, NULL)) != 0)
		{		
			if(errorCode == AVERROR_EXIT) {

				throw gcnew System::OperationCanceledException(token);

			} else {

				throw gcnew VideoLib::VideoLibException("Unable to open the stream:" + VideoInit::errorToString(errorCode));
			}
		}
				
		// generate pts?? -- from ffplay, not documented
		// it should make av_read_frame() generate pts for unknown value
		formatContext->flags |= AVFMT_FLAG_GENPTS;
		
		if((errorCode = avformat_find_stream_info(formatContext, NULL)) < 0)
		{		
			if(errorCode == AVERROR_EXIT) {

				throw gcnew System::OperationCanceledException(token);

			} else {

				throw gcnew VideoLib::VideoLibException("Unable to find the stream's info: " + VideoInit::errorToString(errorCode));
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
					throw gcnew VideoLib::VideoLibException("Unsupported decoder for input stream");
				}
			}

			VideoLib::Stream *newStream = new VideoLib::Stream(formatContext->streams[i], decoder);			
		
			stream.push_back(newStream);			
		}

		videoIdx = av_find_best_stream(formatContext, AVMEDIA_TYPE_VIDEO, -1, -1, NULL, 0);
		if(videoIdx >= 0) {
					
			stream[videoIdx]->open();
			stream[videoIdx]->getCodecContext()->skip_frame = AVDISCARD_DEFAULT; 	
		}
			
		audioIdx = av_find_best_stream(formatContext, AVMEDIA_TYPE_AUDIO, -1, -1, NULL, 0);
		if(audioIdx >= 0) {
				
			stream[audioIdx]->open();		
		}
			
		frame = avcodec_alloc_frame();
		if(frame == NULL)
		{		
			throw gcnew VideoLib::VideoLibException("Unable to allocate frame memory");
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

	}

	int getSizeBytes() const {

		return(sizeBytes);
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
	bool readFrame(AVPacket *packet) {

		int read = av_read_frame(getFormatContext(), packet);
		if(read < 0) return(false);
		else return(true);
	}

	bool decodeVideoFrame(AVFrame *picture, int *got_picture_ptr, const AVPacket *avpkt) {

		int ret = avcodec_decode_video2(getVideoCodecContext(), 
					picture, got_picture_ptr, avpkt);
		if(ret < 0) {

			Video::writeToLog(AV_LOG_WARNING, "could not decode video frame");
			return(false);
		}

		return(true);
	}

	int decodeAudioFrame(AVFrame *audio, int *got_audio_ptr, const AVPacket *avpkt) {

		int ret = avcodec_decode_audio4(getAudioCodecContext(), 
						audio, got_audio_ptr, avpkt);	
		if(ret < 0) {

			Video::writeToLog(AV_LOG_WARNING, "could not decode audio frame");			
		}

		return(ret);
	}

	bool decodeFrame(VideoDecodeMode videoMode = DECODE_VIDEO, 
		AudioDecodeMode audioMode = DECODE_AUDIO, System::Threading::CancellationToken ^token = nullptr, int timeOutSeconds = 0) 
	{

		if(isClosed()) return(false);

		int frameFinished = 0;

		DateTime startTime = DateTime::Now;

		while(!frameFinished) {
		
			if(timeOutSeconds > 0 && (DateTime::Now - startTime).TotalSeconds > timeOutSeconds) {

				throw gcnew VideoLibException("Timed out during decoding");
			}

			if(token != nullptr && token->IsCancellationRequested) {

				throw gcnew VideoLibException("Decoding cancelled");
			}

			if(readFrame(&packet) == false) {

				// cannot read frame or end of file				
				return(false);
			}

			// only decode video/keyframe or non corrupt packets
			if(packet.stream_index == videoIdx && videoMode != SKIP_VIDEO)
			{
				avcodec_get_frame_defaults(frame);

				int ret = avcodec_decode_video2(getVideoCodecContext(), frame, &frameFinished, &packet);
				if(ret < 0) {

					//Error decoding video frame
					//return(0);
				}

				if(frameFinished)
				{										
					convertVideoFrame(frame, convertedFrame);				

					if(decodedFrame != NULL) {
						decodedFrame(data, &packet, convertedFrame, VIDEO);
					}

				}

			} else if(packet.stream_index == audioIdx && audioMode == DECODE_AUDIO) {

					avcodec_get_frame_defaults(frame);

					int ret = avcodec_decode_audio4(getAudioCodecContext(), frame, &frameFinished, 
						&packet);

					if(ret < 0) {
						//Error decoding audio frame
						//return(0);
					}

					if(frameFinished)
					{		
						if(decodedFrame != NULL) {
							decodedFrame(data, &packet, frame, AUDIO);				
						}
					}
			}

			av_free_packet(&packet);
		}

		return(true);

	}


	bool seek(double posSeconds, int flags = 0) {

		// convert timestamp into a videostream timestamp
		AVRational myAVTIMEBASEQ = {1, AV_TIME_BASE}; 
	
		int64_t seekTarget = posSeconds / av_q2d(myAVTIMEBASEQ);
					
		//int ret = av_seek_frame(formatContext, -1, seekTarget, 0);
		int ret = avformat_seek_file(formatContext, -1, 0, seekTarget, seekTarget, flags);
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

			avcodec_free_frame(&convertedFrame);
			convertedFrame = NULL;
		}

		convertedFrame = avcodec_alloc_frame();
		if(convertedFrame == NULL)
		{
			throw gcnew VideoLib::VideoLibException("Unable to allocate frame memory");
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

			throw gcnew VideoLib::VideoLibException("Unable to allocate video convert context");
		} 
		
		// convert frame to the right format
		sws_scale(imageConvertContext,
			input->data,
			input->linesize,
			0,
			input->height,
			output->data,
			output->linesize);	
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

				throw gcnew VideoLib::VideoLibException("Unable to allocate audio convert context");
			}

			if(swr_init(audioConvertContext) != 0)
			{
				throw gcnew VideoLib::VideoLibException("Unable to initialize audio convert context");
			}

		}
	
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
					
		return(length);
	}
	
};

}