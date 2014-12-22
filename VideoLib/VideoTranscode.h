#pragma once
#include "VideoDecoder.h"
#include "VideoEncoder.h"
#include "FilterGraph.h"

using namespace MediaViewer::Infrastructure::Video::TranscodeOptions;
using namespace System::Collections::Generic;
using namespace msclr::interop;

namespace VideoLib {

class VideoTranscode {


protected:

	VideoDecoder *input;
	VideoEncoder *output;

	std::vector<int> streamMap;
	std::vector<FilterGraph *> filterGraph;

public:

	typedef void (__stdcall *ProgressCallback)(double);

	VideoTranscode() {

		input = new VideoDecoder();
		output = new VideoEncoder();		
	}

	~VideoTranscode()
	{
		delete input;
		delete output;
	}

	static void logCallback(int level, const char *message) {
		
		Console::WriteLine(gcnew System::String(message)); 
	}

	void listEncoders() {

		AVCodec *codec = av_codec_next(NULL);

		while(codec != NULL) {

			if(codec->encode2 != NULL && codec->name != NULL) {

				System::Diagnostics::Debug::Print(marshal_as<String ^>(std::string(codec->name)));
			}

			codec = av_codec_next(codec);
		}
	}

	void transcode(String ^inputFilename, String ^outputFilename, System::Threading::CancellationToken ^token, 
		Dictionary<String ^,Object ^> ^options, ProgressCallback progressCallback = NULL) 
	{

		//listEncoders();

		Video::enableLibAVLogging(AV_LOG_INFO);
		Video::setLogCallback((LOG_CALLBACK)logCallback);

		AVPacket packet;
		packet.data = NULL;
		packet.size = 0;

		AVFrame *frame = NULL;
		AVMediaType type;
		int ret = 0;
		int (*dec_func)(AVCodecContext *, AVFrame *, int *, const AVPacket *);

		if(options->ContainsKey("offset")) {

			int offsetSeconds = (int)options["offset"];

			input->seek(offsetSeconds);
		}

		unsigned int stream_index;		
		int got_frame;
		
		try {

			initialize(inputFilename, outputFilename, options);
			
			int i = 0;
			double progress = 0;

			// read all packets 
			while (1) {

				if(token->IsCancellationRequested) {

					throw gcnew VideoLib::VideoLibException("Cancelled transcoding: " + outputFilename);
				}

				if ((ret = av_read_frame(input->getFormatContext(), &packet)) < 0) {
					// finished
					break;
				}

				if(i++ % 500 && progressCallback != NULL) {
					
					double newProgress = fabs(packet.pos / (double)input->getSizeBytes());

					if(newProgress > progress) {

						progressCallback(newProgress);
						progress = newProgress;
					}

				}

				stream_index = packet.stream_index;
				type = input->getFormatContext()->streams[packet.stream_index]->codec->codec_type;

				if(streamMap[stream_index] == -1) {

					// discard packets from this input stream

				} else if (filterGraph[stream_index] != NULL) {
					
					frame = av_frame_alloc();
					if (!frame) {

						throw gcnew VideoLib::VideoLibException("Out of memory");	
					}

					packet.dts = av_rescale_q_rnd(packet.dts,
						input->getFormatContext()->streams[stream_index]->time_base,
						input->getFormatContext()->streams[stream_index]->codec->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

					packet.pts = av_rescale_q_rnd(packet.pts,
						input->getFormatContext()->streams[stream_index]->time_base,
						input->getFormatContext()->streams[stream_index]->codec->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

					dec_func = (type == AVMEDIA_TYPE_VIDEO) ? avcodec_decode_video2 : avcodec_decode_audio4;

					ret = dec_func(input->getFormatContext()->streams[stream_index]->codec, frame, &got_frame, &packet);
					if (ret < 0) {

						av_frame_free(&frame);
						av_log(NULL, AV_LOG_ERROR, "Decoding failed\n");
						break;
					}

					if (got_frame) {

						frame->pts = av_frame_get_best_effort_timestamp(frame);

						ret = filterEncodeWriteFrame(frame, stream_index);
						av_frame_free(&frame);
						if (ret < 0) {
							return;
						}
													
					} else {

						av_frame_free(&frame);
					}

				} else {

					int out_stream_index = streamMap[stream_index];
				
					packet.stream_index = out_stream_index;

					// remux this frame without reencoding 
					packet.dts = av_rescale_q_rnd(packet.dts,
						input->getFormatContext()->streams[stream_index]->time_base,
						output->getFormatContext()->streams[out_stream_index]->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF | AV_ROUND_PASS_MINMAX));

					packet.pts = av_rescale_q_rnd(packet.pts,
						input->getFormatContext()->streams[stream_index]->time_base,
						output->getFormatContext()->streams[out_stream_index]->time_base,
						(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

					packet.duration = av_rescale_q(packet.duration, 
						input->getFormatContext()->streams[stream_index]->time_base, 
						output->getFormatContext()->streams[out_stream_index]->time_base);

					packet.pos = -1;

					ret = av_interleaved_write_frame(output->getFormatContext(), &packet);
					if (ret < 0) {
					
						throw gcnew VideoLib::VideoLibException("Error muxing packet: " + outputFilename);						
					}

				}

				av_free_packet(&packet);
			}

			// flush filters and encoders 
			for(unsigned int i = 0; i < input->getFormatContext()->nb_streams; i++) 
			{
				// flush filter 
				if (filterGraph[i] == NULL) continue;
				ret = filterEncodeWriteFrame(NULL, i);
				if (ret < 0) {

					throw gcnew VideoLib::VideoLibException("Flushing filter failed.");			
				}

				// flush encoder 
				ret = flushEncoder(i);
				if (ret < 0) {

					throw gcnew VideoLib::VideoLibException("Flushing encoder failed.");					
				}
			}

			av_write_trailer(output->getFormatContext());

		} finally {

			if(packet.data != NULL) {

				av_free_packet(&packet);
			}

			av_frame_free(&frame);

			for (unsigned int i = 0; i < filterGraph.size(); i++) {

				if(filterGraph[i] != NULL) {

					delete filterGraph[i];
					filterGraph[i] = NULL;
				}
			
			}
		
			input->close();
			output->close();
		}		

	}

protected:

	void initVideoSettings(const AVCodec* encoder, AVStream *outStream, AVCodecContext *dec_ctx,
		Dictionary<String ^, Object ^> ^options) {

		AVCodecContext *enc_ctx = outStream->codec;

		int width = dec_ctx->width;
		int height = dec_ctx->height;

		if(options->ContainsKey("width")) {

			width = (int)options["width"];

			if(!options->ContainsKey("height")) {

				height = int((dec_ctx->width / (float)width) * dec_ctx->height);
			}
		}

		if(options->ContainsKey("height")) {

			height = (int)options["height"];

			if(!options->ContainsKey("width")) {

				width = int((dec_ctx->height / (float)height) * dec_ctx->width);
			}
		}

		enc_ctx->width = width;
		enc_ctx->height = height;		
		enc_ctx->sample_aspect_ratio = dec_ctx->sample_aspect_ratio;
		// take first format from list of supported formats 
		enc_ctx->pix_fmt = encoder->pix_fmts[0];

		// video time_base can be set to whatever is handy and supported by encoder 					
		outStream->time_base = enc_ctx->time_base = dec_ctx->time_base;	
			
		if(enc_ctx->codec_id == AV_CODEC_ID_H264 || enc_ctx->codec_id == AV_CODEC_ID_HEVC) {
						
			String ^preset = ((VideoEncoderPresets)options["videoEncoderPreset"]).ToString()->ToLower();

			std::string value = marshal_as<std::string>(preset);

			int ret = av_opt_set(enc_ctx->priv_data, "preset", value.c_str(), 0);
			if(ret != 0) {

				throw gcnew VideoLib::VideoLibException("Error setting video options");
			}

		} else if(enc_ctx->codec_id == AV_CODEC_ID_VP8) {
			
			enc_ctx->bit_rate = 1000000;

			av_opt_set(enc_ctx, "crf", "10", 0);
			av_opt_set(enc_ctx, "qmin", "4", 0);
			av_opt_set(enc_ctx, "qmax", "50", 0);

		}
	}

	void initAudioSettings(const AVCodec* encoder, AVStream *outStream, AVCodecContext *dec_ctx,
		Dictionary<String ^, Object ^> ^options) 
	{

		AVCodecContext *enc_ctx = outStream->codec;

		int sampleRate = dec_ctx->sample_rate;

		if(options->ContainsKey("sampleRate")) {

			sampleRate = (int)options["sampleRate"];
		}

		enc_ctx->sample_rate = sampleRate;
		enc_ctx->channel_layout = dec_ctx->channel_layout;
		enc_ctx->channels = av_get_channel_layout_nb_channels(enc_ctx->channel_layout);
		// take first format from list of supported formats 
		enc_ctx->sample_fmt = encoder->sample_fmts[0];

		outStream->time_base.num = 1;
		outStream->time_base.den = enc_ctx->sample_rate;
	
		enc_ctx->time_base = outStream->time_base;
	}


	AVCodecContext *initStream(AVMediaType type, int inputStreamIndex, String ^outputFilename, 
		StreamOptions streamMode, Dictionary<String ^, Object ^> ^options) {

		if(streamMode == StreamOptions::Discard) {

			streamMap.push_back(-1);		
			return(NULL);
		}

		int ret = 0;

		AVStream *in_stream = input->getFormatContext()->streams[inputStreamIndex];
		AVCodecContext *dec_ctx = in_stream->codec;
							
		AVStream *out_stream;
		AVCodec *encoder;

		VideoLib::Stream *outStream;

		if(streamMode == StreamOptions::Copy) {

			encoder = avcodec_find_encoder(dec_ctx->codec->id);
			if(encoder == NULL) {
		
				throw gcnew VideoLib::VideoLibException("Could not find suitable encoder for stream: " + outputFilename);
			}
		
			out_stream = avformat_new_stream(output->getFormatContext(), encoder);
			if (out_stream == NULL) {
			
				throw gcnew VideoLib::VideoLibException("Could not create output stream for: " + outputFilename);
			}

			ret = avcodec_copy_context(out_stream->codec, dec_ctx);
			if (ret < 0) {

				throw gcnew VideoLib::VideoLibException("Copying stream context failed: " + outputFilename);							
			}

			out_stream->codec->codec_tag = 0;

			outStream = new VideoLib::Stream(out_stream, encoder);
			output->addStream(outStream);

		} else {

			if(type == AVMEDIA_TYPE_VIDEO) {

				std::string encoderName = marshal_as<std::string>(((VideoEncoders)options["videoEncoder"]).ToString());
				 
				encoder = avcodec_find_encoder_by_name(encoderName.c_str());
				if(encoder == NULL) {
		
					throw gcnew VideoLib::VideoLibException("Could not find suitable encoder for stream: " + outputFilename);
				}

				out_stream = avformat_new_stream(output->getFormatContext(), encoder);
				if (out_stream == NULL) {
			
					throw gcnew VideoLib::VideoLibException("Could not create output stream for: " + outputFilename);
				}

				initVideoSettings(encoder, out_stream, dec_ctx, options);
				
			} else {

				std::string encoderName = marshal_as<std::string>(((AudioEncoders)options["audioEncoder"]).ToString());

				encoder = avcodec_find_encoder_by_name(encoderName.c_str());
				if(encoder == NULL) {
		
					throw gcnew VideoLib::VideoLibException("Could not find suitable encoder for stream: " + outputFilename);
				}

				out_stream = avformat_new_stream(output->getFormatContext(), encoder);
				if (out_stream == NULL) {
			
					throw gcnew VideoLib::VideoLibException("Could not create output stream for: " + outputFilename);
				}

				initAudioSettings(encoder, out_stream, dec_ctx, options);			
			}
					
			outStream = new VideoLib::Stream(out_stream, encoder);
			output->addStream(outStream);

			outStream->open();
		}
		
		streamMap.push_back(output->getFormatContext()->nb_streams - 1);

		AVCodecContext *enc_ctx = outStream->getCodecContext();		
	
		return(enc_ctx);
	}

	
	void initialize(String ^inputFilename, String ^outputFilename, Dictionary<String ^,Object ^> ^options) {

		int ret = 0;
		
		input->open(inputFilename);					
		output->open(outputFilename);

		filterGraph.clear();
		streamMap.clear();

		for(unsigned int i = 0; i < input->getFormatContext()->nb_streams; i++) {
								
			AVStream *in_stream = input->getFormatContext()->streams[i];
			AVCodecContext *dec_ctx = in_stream->codec;

			AVCodecContext *enc_ctx = NULL;
		
			if (dec_ctx->codec_type == AVMEDIA_TYPE_VIDEO || dec_ctx->codec_type == AVMEDIA_TYPE_AUDIO) {

				StreamOptions streamMode = dec_ctx->codec_type == AVMEDIA_TYPE_VIDEO ? 
					(StreamOptions)options["videoStreamMode"] : (StreamOptions)options["audioStreamMode"];
							
				enc_ctx = initStream(dec_ctx->codec_type, i, outputFilename, streamMode, options);

				if (output->getFormatContext()->oformat->flags & AVFMT_GLOBALHEADER && enc_ctx != NULL) {

					enc_ctx->flags |= CODEC_FLAG_GLOBAL_HEADER;
				}
				
			} else if (dec_ctx->codec_type == AVMEDIA_TYPE_UNKNOWN) {

				throw gcnew VideoLib::VideoLibException("Elementary stream is of unknown type, cannot proceed: " + outputFilename);

			} else {

				streamMap.push_back(-1);

				// if this stream must be remuxed 
				/*AVStream *out_stream = avformat_new_stream(output->getFormatContext(), NULL);
				if (!out_stream) {

					throw gcnew VideoLib::VideoLibException("Could not create new stream: " + outputFilename);
				}

				enc_ctx = out_stream->codec;	
				
				ret = avcodec_copy_context(enc_ctx, dec_ctx);
				if (ret < 0) {

					throw gcnew VideoLib::VideoLibException("Copying stream context failed: " + outputFilename);							
				}*/
				
			}

			
		}

		av_dump_format(output->getFormatContext(), 0, output->getFormatContext()->filename, 1);

		if (!(output->getFormatContext()->oformat->flags & AVFMT_NOFILE)) {

			ret = avio_open(&output->getFormatContext()->pb, output->getFormatContext()->filename, AVIO_FLAG_WRITE);
			if (ret < 0) {

				throw gcnew VideoLib::VideoLibException("Could not open output file: " + outputFilename);										
			}
		}

		// init muxer, write output file header 
		ret = avformat_write_header(output->getFormatContext(), NULL);
		if (ret < 0) {

			throw gcnew VideoLib::VideoLibException("Error writing header: " + outputFilename + " " + VideoInit::errorToString(ret));	

		}

		initFilters(options);
	}

	int flushEncoder(unsigned int stream_index)
	{
		int ret;
		int got_frame;

		int out_stream_index = streamMap[stream_index];

		if (!(output->getFormatContext()->streams[out_stream_index]->codec->codec->capabilities & CODEC_CAP_DELAY))
		{
			return 0;
		}

		while (1) {

			ret = encodeWriteFrame(NULL, stream_index, &got_frame);

			if (ret < 0) break;

			if (!got_frame) return 0;
		}

		return ret;
	}

	int encodeWriteFrame(AVFrame *filt_frame, unsigned int stream_index, int *got_frame) {

		int ret;
		int got_frame_local;

		AVPacket enc_pkt;

		int (*enc_func)(AVCodecContext *, AVPacket *, const AVFrame *, int *) =
			(input->getFormatContext()->streams[stream_index]->codec->codec_type ==
			AVMEDIA_TYPE_VIDEO) ? avcodec_encode_video2 : avcodec_encode_audio2;

		if (!got_frame)
			got_frame = &got_frame_local;

		//av_log(NULL, AV_LOG_INFO, "Encoding frame\n");
		// encode filtered frame 
		enc_pkt.data = NULL;
		enc_pkt.size = 0;

		av_init_packet(&enc_pkt);

		int out_stream_index = streamMap[stream_index];

		ret = enc_func(output->getFormatContext()->streams[out_stream_index]->codec, &enc_pkt, filt_frame, got_frame);
		av_frame_free(&filt_frame);
		if (ret < 0) {

			String ^error = VideoInit::errorToString(ret);
			return ret;
		}

		if (!(*got_frame))
			return 0;
			
		// prepare packet for muxing 
		enc_pkt.stream_index = out_stream_index;

		enc_pkt.dts = av_rescale_q_rnd(enc_pkt.dts,
			output->getFormatContext()->streams[out_stream_index]->codec->time_base,
			output->getFormatContext()->streams[out_stream_index]->time_base,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		enc_pkt.pts = av_rescale_q_rnd(enc_pkt.pts,
			output->getFormatContext()->streams[out_stream_index]->codec->time_base,
			output->getFormatContext()->streams[out_stream_index]->time_base,
			(AVRounding)(AV_ROUND_NEAR_INF|AV_ROUND_PASS_MINMAX));

		enc_pkt.duration = av_rescale_q(enc_pkt.duration,
			output->getFormatContext()->streams[out_stream_index]->codec->time_base,
			output->getFormatContext()->streams[out_stream_index]->time_base);
		//av_log(NULL, AV_LOG_DEBUG, "Muxing frame\n");
		// mux encoded frame 
		

		ret = av_interleaved_write_frame(output->getFormatContext(), &enc_pkt);


		return ret;
	}

	int filterEncodeWriteFrame(AVFrame *frame, unsigned int stream_index)
	{
		int ret;
		AVFrame *filt_frame;
	
		/* push the decoded frame into the filtergraph */

		ret = av_buffersrc_add_frame_flags(filterGraph[stream_index]->getBufferSourceContext(), frame, 0);
		if (ret < 0) {

			throw gcnew VideoLib::VideoLibException("Error while feeding the filtergraph");			
		}

		// pull filtered frames from the filtergraph
		while (1) {

			filt_frame = av_frame_alloc();
			if (!filt_frame) {

				throw gcnew VideoLib::VideoLibException("Not enough memory");				
			}

			ret = av_buffersink_get_frame(filterGraph[stream_index]->getBufferSinkContext(), filt_frame);
			if (ret < 0) {
				/* if no more frames for output - returns AVERROR(EAGAIN)
				* if flushed and no more frames for output - returns AVERROR_EOF
				* rewrite retcode to 0 to show it as normal procedure completion
				*/
				if (ret == AVERROR(EAGAIN) || ret == AVERROR_EOF) {

					ret = 0;
				}
				av_frame_free(&filt_frame);
				break;
			}

			filt_frame->pict_type = AV_PICTURE_TYPE_NONE;
			ret = encodeWriteFrame(filt_frame, stream_index, NULL);
			if (ret < 0) {
				break;
			}
		}

		return ret;
	}

	int initFilters(Dictionary<String ^,Object ^> ^options)
	{		
		unsigned int i;
						
		for (i = 0; i < input->getFormatContext()->nb_streams; i++) {
					
			filterGraph.push_back(NULL);
			
			if (input->getFormatContext()->streams[i]->codec->codec_type == AVMEDIA_TYPE_VIDEO) {

				if((StreamOptions)options["videoStreamMode"] != StreamOptions::Encode) continue;

				filterGraph[i] = new FilterGraph(input->getVideoCodecContext(),
					output->getVideoCodecContext()->pix_fmt, SWS_BICUBIC);

				int outputFmt = (int)output->getVideoCodecContext()->pix_fmt;
				int outWidth = output->getVideoCodecContext()->width;
				int outHeight = output->getVideoCodecContext()->height;

				String ^graph = "scale=" + outWidth + "x" + outHeight + ",format=" + outputFmt;
				
				std::string value = msclr::interop::marshal_as<std::string>(graph);

				filterGraph[i]->createGraph(value.c_str());				

			} else {

				if((StreamOptions)options["audioStreamMode"] != StreamOptions::Encode) continue;

				filterGraph[i] = new FilterGraph(input->getAudioCodecContext(),
					output->getAudioCodecContext()->sample_fmt, output->getAudioCodecContext()->channel_layout,
					output->getAudioCodecContext()->sample_rate);

				filterGraph[i]->createGraph("anull");	
			}
			
		}

		return 0;
	}

	

};

}