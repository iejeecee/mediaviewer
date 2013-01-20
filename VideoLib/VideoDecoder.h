#pragma once
#include "stdafx.h"
#include "Video.h"

class VideoDecoder : public Video {

protected:

	AVFrame *frame;

	AVAudioConvert *audioConvert;

	SwsContext *imageConvertContext;

	AVFrame *convertedFrame;
	uint8_t *convertedFrameBuffer;

	double startTime;

	void *data;

	void (*decodedFrame)(void *data, AVPacket *packet, AVFrame *frame, FrameType type);

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
		convertedFrame = NULL;
		convertedFrameBuffer = NULL;

		startTime = 0;
			
	}

	virtual ~VideoDecoder() {

		close();
	}

	void setDecodedFrameCallback(void (*decodedFrame)(void *data, AVPacket *packet, AVFrame *frame, FrameType type) = NULL,
		void *data = NULL) 
	{
		this->decodedFrame = decodedFrame;
		this->data = data;
	}

	virtual void close() {

		Video::close();

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

		startTime = 0;
	
	}

	std::string getFileName() const {

		return(formatContext->filename);
	}

	virtual void open(const std::string &location, AVDiscard discardMode = AVDISCARD_DEFAULT) {

		int errorCode = 0;

		if((errorCode = avformat_open_input(&formatContext, location.c_str(), NULL, NULL)) != 0)
		{
			throw std::runtime_error("Unable to open the stream:");
		}

		// generate pts?? -- from ffplay, not documented
		// it should make av_read_frame() generate pts for unknown value
		formatContext->flags |= AVFMT_FLAG_GENPTS;

		if((errorCode = avformat_find_stream_info(formatContext, NULL)) < 0)
		{
			throw std::runtime_error("Unable to find the stream's info");
		}

		for(unsigned int i = 0; i < formatContext->nb_streams; ++i)
		{
			if(formatContext->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO)
			{				
				videoStreamIndex = i;
				videoStream = formatContext->streams[videoStreamIndex];
			
			} else if(formatContext->streams[i]->codec->codec_type == AVMEDIA_TYPE_AUDIO) {

				audioStreamIndex = i;
				audioStream = formatContext->streams[audioStreamIndex];
			}
		}

		if(videoStreamIndex == -1)
		{
			throw std::runtime_error("Unable to find a video stream");
		}

		videoCodecContext = formatContext->streams[videoStreamIndex]->codec;
		videoCodec = avcodec_find_decoder(videoCodecContext->codec_id);
		if(videoCodec == NULL)
		{
			throw std::runtime_error("Unsupported videoCodec");
		}

		videoCodecContext->skip_frame = discardMode;

		if ((errorCode = avcodec_open2(videoCodecContext, videoCodec, NULL)) != 0)
		{
			throw std::runtime_error("Error opening the videoCodec");
		}

		if(audioStreamIndex != -1) {

			audioCodecContext = formatContext->streams[audioStreamIndex]->codec;
			audioCodec = avcodec_find_decoder(audioCodecContext->codec_id);

			if(audioCodec == NULL) {

				std::cerr << "no suitable audio decoder found\n";
			}

			if ((errorCode = avcodec_open2(audioCodecContext, audioCodec, NULL)) != 0)
			{
				throw std::runtime_error("Error opening the audioCodec");
			}
		}

		
		//TODO: if one of these fail, we should release what has succeeded
		frame = avcodec_alloc_frame();

		if(frame == NULL)
		{
			throw std::runtime_error("Unable to allocate frame memory");
		}

		if(formatContext->start_time == AV_NOPTS_VALUE) {

			startTime = 0;

		} else {

			startTime = formatContext->start_time / AV_TIME_BASE;
		}

		// int64_t duration_tb = duration / av_q2d(pStream->time_base); 
	}

	double getDurationSeconds() const {

		double videoDuration  = formatContext->duration / AV_TIME_BASE;
		
		if(videoDuration < 0) {

			videoDuration = videoStream->duration * av_q2d(videoStream->time_base);
		}
		
		if(videoDuration < 0) {

			throw std::runtime_error("can't determine video duration");
		}

		return(videoDuration);

	}

	int decode(VideoDecodeMode videoMode = DECODE_VIDEO, 
		AudioDecodeMode audioMode = DECODE_AUDIO,
		int nrFrames = -1) 
	{

		int nrDecodedFrames = 0;

		while(nrFrames != 0)
		{

			AVPacket packet;

			if(av_read_frame(formatContext, &packet) != 0) {

				break;
			}

			int frameFinished = 0;

			// only decode video/keyframe or non corrupt packets
			if((packet.stream_index == videoStreamIndex) &&
				(videoMode != DECODE_KEY_FRAMES_ONLY || (packet.flags & AV_PKT_FLAG_KEY)) &&
				!(packet.flags & AV_PKT_FLAG_CORRUPT) &&
				(videoMode != SKIP_VIDEO))
			{

				avcodec_get_frame_defaults(frame);

				avcodec_decode_video2(videoCodecContext, frame, &frameFinished, &packet);

				if(frameFinished)
				{		

					AVFrame *finishedFrame = frame;

					if(imageConvertContext != NULL) {

						sws_scale(imageConvertContext,
							frame->data,
							frame->linesize,
							0,
							frame->height,
							convertedFrame->data,
							convertedFrame->linesize);

						finishedFrame = convertedFrame;
					}

					if(decodedFrame != NULL) {
						decodedFrame(data, &packet, finishedFrame, VIDEO);
					}
				
					nrDecodedFrames++;
					if(nrFrames > 0) nrFrames--;
				}

			} else if((packet.stream_index == audioStreamIndex) &&
					(audioMode == DECODE_AUDIO)) {

				avcodec_get_frame_defaults(frame);
				
				avcodec_decode_audio4(audioCodecContext, frame, &frameFinished, 
					&packet);
				
				if(frameFinished)
				{		
					if(decodedFrame != NULL) {
						decodedFrame(data, &packet, frame, AUDIO);				
					}
				}
			}

			av_free_packet(&packet);

		}

		return(nrDecodedFrames);
	}

	int seek(double posSeconds) {

		// convert timestamp into a videostream timestamp
		//AVRational myAVTIMEBASEQ = {1, AV_TIME_BASE}; 

		//AVRational timeBase = videoStream->time_base;

		//int64_t seekTarget = av_rescale_q(timeStamp, myAVTIMEBASEQ , timeBase);
		//int64_t seekTarget = av_rescale(timeStamp, timeBase.num, timeBase.den);

		int64_t seekTarget = posSeconds / av_q2d(videoStream->time_base);
		//int64_t seekTarget2 = av_rescale(posSeconds, timeBase.num, timeBase.den);
		//std::cout << "Seek target 2: " <<  double(seekTarget * av_q2d(videoStream->time_base)) / 60 << "\n";

		int ret;

		// first try av_seek_frame 
		ret = av_seek_frame(formatContext, videoStreamIndex, seekTarget, AVSEEK_FLAG_BACKWARD);
		//ret = av_seek_frame(formatContext, -1, timeStamp, 0);
		if (ret >= 0) { // success

			avcodec_flush_buffers(videoCodecContext);
			return ret;
		}

		// then we try seeking to any (non key) frame AVSEEK_FLAG_ANY 
		ret = av_seek_frame(formatContext, videoStreamIndex, seekTarget, AVSEEK_FLAG_ANY);
		if (ret >= 0) { // success
			
			avcodec_flush_buffers(videoCodecContext);
			return ret;			
		}

		throw std::runtime_error("Unable to seek in stream");
		return -1;
/*
		// and then we try seeking by byte (AVSEEK_FLAG_BYTE) 
		// here we assume that the whole file has duration seconds.
		// so we'll interpolate accordingly.
		AVStream *pStream = formatContext->streams[videoStreamIndex];
		int64_t duration_tb = duration / av_q2d(pStream->time_base); // in time_base unit
		double start_time = (double) formatContext->start_time / AV_TIME_BASE; // in seconds
		// if start_time is negative, we ignore it; FIXME: is this ok?
		if (start_time < 0) {
			start_time = 0;
		}

		// normally when seeking by seekTarget we add start_time to seekTarget 
		// before seeking, but seeking by byte we need to subtract the added start_time
		seekTarget -= start_time / av_q2d(pStream->time_base);
		if (formatContext->file_size <= 0) {
			return -1;
		}
		if (duration > 0) {
			int64_t byte_pos = av_rescale(seekTarget, formatContext->file_size, duration_tb);
			av_log(NULL, LOG_INFO, "AVSEEK_FLAG_BYTE: byte_pos: %"PRId64", seekTarget: %"PRId64", file_size: %"PRId64", duration_tb: %"PRId64"\n", byte_pos, seekTarget, formatContext->file_size, duration_tb);
			return av_seek_frame(formatContext, videoStreamIndex, byte_pos, AVSEEK_FLAG_BYTE);
		}

*/
		
	}

	void initImageConverter(PixelFormat format, int dstWidth, int dstHeight, 
		SamplingMode sampling) 
	{

		convertedFrame = avcodec_alloc_frame();
		if(convertedFrame == NULL)
		{
			throw std::runtime_error("Unable to allocate frame memory");
		}

		int numBytes = avpicture_get_size(format, dstWidth, dstHeight);
		convertedFrameBuffer = (uint8_t*)av_malloc(numBytes);

		avpicture_fill((AVPicture*)convertedFrame, convertedFrameBuffer, format, dstWidth, dstHeight);

		imageConvertContext = sws_getContext(
			videoCodecContext->width,
			videoCodecContext->height,
			videoCodecContext->pix_fmt,
			dstWidth,
			dstHeight,
			format,
			sampling,
			NULL,
			NULL,
			NULL);
	}

	void InitAudioConverter(AVSampleFormat outFormat, int outNrChannels = 2) {

		if(audioCodec == NULL) {

			std::cerr << "Cannot create audio converter without input audio\n";
			return;
		}

		//enum AVSampleFormat out_fmt, int out_channels,

	}

	int getWidth() const {

		return(videoCodecContext == NULL ? 0 : videoCodecContext->width);
	}

	int getHeight() const {

		return(videoCodecContext == NULL ? 0 : videoCodecContext->height);
	}

	
};

