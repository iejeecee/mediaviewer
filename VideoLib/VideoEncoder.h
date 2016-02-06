#pragma once
#include "stdafx.h"
#include "Video.h"
#include "VideoLibException.h"
#include "MemoryStreamAVIOContext.h"

using namespace msclr::interop;

namespace VideoLib {

class VideoEncoder : public Video {

protected:
	
	AVOutputFormat *outFormat;

	virtual void addStream(VideoLib::Stream *stream) {

		_ASSERT(outFormat != NULL);

		Video::addStream(stream);

		if (outFormat->flags & AVFMT_GLOBALHEADER && stream->getCodecContext() != NULL) {

			stream->getCodecContext()->flags |= CODEC_FLAG_GLOBAL_HEADER;
		}
	}

	void validateOutputStream(const VideoLib::Stream *stream) {
						
		if(stream->getCodec()->pix_fmts != NULL) {
				
			int i = 0;
			bool isPixelFormatSupported = false;
			
			while(stream->getCodec()->pix_fmts[i] != -1) {

				if(stream->getCodecContext()->pix_fmt == stream->getCodec()->pix_fmts[i]) {
					
					isPixelFormatSupported = true;
					break;			
				}			

				i++;
			}

			if(isPixelFormatSupported == false && i > 1)
			{
				throw gcnew VideoLib::VideoLibException("Pixel format not supported for output stream " + 
					gcnew String(Utils::getPixelformatName(stream->getCodecContext()->pix_fmt).c_str()));
			}
		} 

		const AVRational *supportedFrameRate = stream->getCodec()->supported_framerates;
				
		if(stream->getCodec()->supported_framerates != NULL) {

			int i = 0;

			bool isFrameRateSupported = false;

			while(stream->getCodec()->supported_framerates[i].den != 0)
			{
				if(av_cmp_q(stream->getCodec()->supported_framerates[i], stream->getTimeBase()) == 0) {

					isFrameRateSupported = true;
					break;
				}

				i++;
			}
			
			if(isFrameRateSupported == false && i > 1)
			{
				throw gcnew VideoLib::VideoLibException("framerate not supported for output stream " + stream->getTimeBase().den + "fps");
			}
		}

		if(stream->getCodec()->supported_samplerates != NULL) {
				
			int i = 0;
			bool isSampleRateSupported = false;
			
			while(stream->getCodec()->supported_samplerates[i] != 0) {

				if(stream->getCodecContext()->sample_rate == stream->getCodec()->supported_samplerates[i]) {
					
					isSampleRateSupported = true;
					break;			
				}		

				i++;
			}

			if(isSampleRateSupported == false && i > 1)
			{
				throw gcnew VideoLib::VideoLibException("Sample rate not supported for output stream " + stream->getCodecContext()->sample_rate + "hz");
			}
		} 
				
		if(stream->getCodec()->sample_fmts != NULL) {
				
			int i = 0;
			bool isSampleFormatSupported = false;
			
			while(stream->getCodec()->sample_fmts[i] != -1) {

				if(stream->getCodecContext()->sample_fmt == stream->getCodec()->sample_fmts[i]) {
					
					isSampleFormatSupported = true;
					break;			
				}				

				i++;
			}

			if(isSampleFormatSupported == false && i > 1)
			{
				throw gcnew VideoLib::VideoLibException("Sample format not supported for output stream " +
					gcnew String(Utils::getSampleFormatName(stream->getCodecContext()->sample_fmt).c_str()));
			}
		} 

		if(stream->getCodec()->channel_layouts != NULL) {
				
			int i = 0;
			bool isChannelLayoutSupported = false;
			
			while(stream->getCodec()->channel_layouts != 0) {

				if(stream->getCodecContext()->channel_layout == stream->getCodec()->channel_layouts[i]) {
					
					isChannelLayoutSupported = true;
					break;			
				}				

				i++;
			}

			if(isChannelLayoutSupported == false && i > 1)
			{
				throw gcnew VideoLib::VideoLibException("Channel layout not supported for output stream " + 
					gcnew String(Utils::getChannelLayoutName(stream->getCodecContext()->channel_layout).c_str()));
			}
		} 

		
		
	}

public: 
	
	VideoEncoder() {

		outFormat = NULL;
		
	}

	virtual ~VideoEncoder() {

		close();
	}

	void open(MemoryStreamAVIOContext *memoryStreamCtx, const char *formatName) {

		//formatContext = avformat_alloc_context();		
		
		avformat_alloc_output_context2(&formatContext, NULL, formatName, NULL);		
		if(formatContext == NULL) {

			throw gcnew VideoLib::VideoLibException("Cannot create output format context with memorystream");
		}
	
		formatContext->flags |= AVFMT_FLAG_CUSTOM_IO;
		formatContext->pb = memoryStreamCtx->getAVIOContext();
		
		outFormat = formatContext->oformat;

	}

	virtual void open(String ^outputFilename, const char *formatName = NULL) {

		// Convert location to UTF8 string pointer
		array<Byte>^ encodedBytes = System::Text::Encoding::UTF8->GetBytes(outputFilename);

		// prevent GC moving the bytes around while this variable is on the stack
		pin_ptr<Byte> pinnedBytes = &encodedBytes[0];

		// Call the function, typecast from byte* -> char* is required
		char *outputUTF8 = reinterpret_cast<char*>(pinnedBytes);
			
		avformat_alloc_output_context2(&formatContext, NULL, formatName, outputUTF8);		
		if(formatContext == NULL) {

			throw gcnew VideoLib::VideoLibException("Cannot create output format context: " + outputFilename);
		}
	
		outFormat = formatContext->oformat;
				
	}

	// Create a new output stream 
	Stream *createStream(Stream *copy) {
		
		_ASSERT(formatContext != NULL);

		AVCodec *encoder = avcodec_find_encoder(copy->getCodec()->id);
		if(encoder == NULL) {
		
			throw gcnew VideoLib::VideoLibException("Could not find suitable encoder for stream: " + marshal_as<String ^>(formatContext->filename));
		}
		
		AVStream *stream = avformat_new_stream(formatContext, encoder);
		if (stream == NULL) {
			
			throw gcnew VideoLib::VideoLibException("Could not create output stream for: " + marshal_as<String ^>(formatContext->filename));
		}
		
		int ret = avcodec_copy_context(stream->codec, copy->getCodecContext());
		if (ret != 0) {

			throw gcnew VideoLib::VideoLibException("Copying stream context failed: " + marshal_as<String ^>(formatContext->filename) + " ", ret);							
		}

		av_stream_set_r_frame_rate(stream, copy->getAVStream()->r_frame_rate);
	
		stream->avg_frame_rate      = copy->getAVStream()->avg_frame_rate;
		stream->time_base           = copy->getAVStream()->time_base;
		stream->sample_aspect_ratio = copy->getAVStream()->sample_aspect_ratio;		

		av_dict_copy(&stream->metadata, copy->getAVStream()->metadata, 0);

		stream->codec->codec_tag = 0;

		/*if(stream->codec->codec_id == AV_CODEC_ID_GIF)
		{
			stream->codec->pix_fmt = encoder->pix_fmts[0];
		}*/

		VideoLib::Stream *newStream = new VideoLib::Stream(stream, encoder);

		addStream(newStream);

		validateOutputStream(newStream);

		return(newStream);
	}

	Stream *createStream(const std::string &encoderName, int width, int height, const AVRational &sampleAspectRatio,
		const AVRational &timeBase, AVPixelFormat pixelFormat = AV_PIX_FMT_NONE) 
	{
		_ASSERT(formatContext != NULL);

		AVCodec *encoder = avcodec_find_encoder_by_name(encoderName.c_str());
		if(encoder == NULL) {
		
			throw gcnew VideoLib::VideoLibException("Could not find suitable encoder for stream: " + marshal_as<String ^>(formatContext->filename));
		}

		AVStream *stream = avformat_new_stream(formatContext, encoder);
		if (stream == NULL) {
			
			throw gcnew VideoLib::VideoLibException("Could not create output stream for: " + marshal_as<String ^>(formatContext->filename));
		}

		AVCodecContext *enc_ctx = stream->codec;

		enc_ctx->width = width;
		enc_ctx->height = height;		
		enc_ctx->sample_aspect_ratio = sampleAspectRatio;
			
		if(pixelFormat == AV_PIX_FMT_NONE) {

			if(encoder->pix_fmts == NULL) {

				throw gcnew VideoLib::VideoLibException("No default pixel format for output stream");
			}

			//take first format from list of supported formats 
			enc_ctx->pix_fmt = encoder->pix_fmts[0];

		} else {
						
			enc_ctx->pix_fmt = pixelFormat;			
		}
		
		// video time_base can be set to whatever is handy and supported by encoder 					
		stream->time_base = enc_ctx->time_base = timeBase;	

		VideoLib::Stream *newStream = new VideoLib::Stream(stream, encoder);

		addStream(newStream);

		validateOutputStream(newStream);

		return(newStream);
	}

	Stream *createStream(const std::string &encoderName, int sampleRate, int nrChannels) 
	{
		_ASSERT(formatContext != NULL);

		AVCodec *encoder = avcodec_find_encoder_by_name(encoderName.c_str());
		if(encoder == NULL) {
		
			throw gcnew VideoLib::VideoLibException("Could not find suitable encoder for stream: " + marshal_as<String ^>(formatContext->filename));
		}

		AVStream *stream = avformat_new_stream(formatContext, encoder);
		if (stream == NULL) {
			
			throw gcnew VideoLib::VideoLibException("Could not create output stream for: " + marshal_as<String ^>(formatContext->filename));
		}

		AVCodecContext *enc_ctx = stream->codec;

		enc_ctx->sample_rate = sampleRate;
		enc_ctx->channels = nrChannels;
		enc_ctx->channel_layout = av_get_default_channel_layout(nrChannels);
		enc_ctx->sample_fmt = encoder->sample_fmts[0];

		stream->time_base.num = 1;
		stream->time_base.den = sampleRate;
	
		enc_ctx->time_base = stream->time_base;
		
		VideoLib::Stream *newStream = new VideoLib::Stream(stream, encoder);

		addStream(newStream);

		validateOutputStream(newStream);

		return(newStream);
	}


	void writeHeader() {

		_ASSERT(formatContext != NULL);

		int ret;
			
		if(formatContext->pb == NULL) {

			ret = avio_open(&formatContext->pb, formatContext->filename, AVIO_FLAG_WRITE);
			if (ret < 0) {

				throw gcnew VideoLib::VideoLibException("Could not open output file: " + marshal_as<String ^>(formatContext->filename) + " ", ret);										
			}
		}
		
		// init muxer, write output file header 
		ret = avformat_write_header(formatContext, NULL);
		if (ret < 0) {

			throw gcnew VideoLib::VideoLibException("Error writing header: " + marshal_as<String ^>(formatContext->filename) + " ", ret);	

		}

	}

	bool encodeFrame(int streamIdx, const AVFrame *frame, AVPacket *encPacket) {

		encPacket->data = NULL;
		encPacket->size = 0;

		av_init_packet(encPacket);

		int ret = 0;
		int gotPacket = 0;
		
		if(stream[streamIdx]->getCodecType() == AVMEDIA_TYPE_VIDEO) 
		{
			ret = avcodec_encode_video2(stream[streamIdx]->getCodecContext(), encPacket, frame, &gotPacket);
		} 
		else if(stream[streamIdx]->getCodecType() == AVMEDIA_TYPE_AUDIO) 
		{
			ret = avcodec_encode_audio2(stream[streamIdx]->getCodecContext(), encPacket, frame, &gotPacket);			
		} 
		else {

			throw gcnew VideoLib::VideoLibException("Trying to encode non audio/video frame: " + marshal_as<String ^>(formatContext->filename));	
		}

		if(ret < 0) {

			throw gcnew VideoLib::VideoLibException("Error encoding frame: " + marshal_as<String ^>(formatContext->filename) + " ", ret);	
		}

		encPacket->stream_index = streamIdx;

		return(gotPacket == 1 ? true : false);
	}

	void writeEncodedPacket(AVPacket *encPacket) {
					
		int ret = av_interleaved_write_frame(formatContext, encPacket);
		if(ret != 0) {

			throw gcnew VideoLib::VideoLibException("Error writing encoded packet for: " + marshal_as<String ^>(formatContext->filename) + " ", ret);	
		}
		
	}

	void writeTrailer() {

		_ASSERT(formatContext != NULL);

		int ret = av_write_trailer(formatContext);
		if(ret != 0) {

			throw gcnew VideoLib::VideoLibException("Error writing trailer for: " + marshal_as<String ^>(formatContext->filename) + " ", ret);	
		}
	}

	virtual void close() {

		 Video::close();

		 if (formatContext != NULL) 
		 {
			if(!(formatContext->flags & AVFMT_FLAG_CUSTOM_IO) && formatContext->pb != NULL) {
				
				avio_close(formatContext->pb);		
			}		

			avformat_free_context(formatContext);
			formatContext = NULL;
		 }
		 
	}

	AVOutputFormat *getOutputFormat() const {

		return(outFormat);
	}
};

}