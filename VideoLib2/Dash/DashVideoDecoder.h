#pragma once
#include "..\Video\IVideoDecoder.h"

namespace VideoLib2 {

	class DashVideoDecoder : public IVideoDecoder {

	protected:

		VideoDecoder *videoDecoder;
		VideoDecoder *audioDecoder;

		int audioStreamOffset;

		double videoDts, audioDts;
		AVPacket videoPacket,audioPacket;

		VideoDecoder::ReadFrameResult readVideoFrame(AVPacket &packet) {
				 
			av_packet_unref(&packet);

			VideoDecoder::ReadFrameResult result = videoDecoder->readFrame(&packet);

			if(result != VideoDecoder::ReadFrameResult::OK) return result;

			if(packet.dts != AV_NOPTS_VALUE) {

				videoDts = videoDecoder->getStream(videoDecoder->getVideoStreamIndex())->getTimeSeconds(packet.dts);

			} else {

				throw gcnew VideoLib2::VideoLibException("Input video stream does not contain valid decoding timestamps");
			}

			return(result);

		}

		VideoDecoder::ReadFrameResult readAudioFrame(AVPacket &packet) {
			
			av_packet_unref(&packet);

			VideoDecoder::ReadFrameResult result = audioDecoder->readFrame(&packet);

			packet.stream_index += audioStreamOffset;

			if(result != VideoDecoder::ReadFrameResult::OK) return result;

			if(packet.dts != AV_NOPTS_VALUE) {

				audioDts = audioDecoder->getStream(audioDecoder->getAudioStreamIndex())->getTimeSeconds(packet.dts);

			} else {

				throw gcnew VideoLib2::VideoLibException("Input audio stream does not contain valid decoding timestamps");
			}

			return(result);
		}

	public:

		VideoLib2::Stream *getStream(int i) const 
		{
			if(i < audioStreamOffset) {

				return(videoDecoder->getStream(i));

			} else {

				return(audioDecoder->getStream(i - audioStreamOffset));

			}
		}

		virtual AVFormatContext *getFormatContext() const {

			return(videoDecoder->getFormatContext());
		}

		virtual int getVideoStreamIndex() const {

			return(videoDecoder->getVideoStreamIndex());
		}

		virtual int getAudioStreamIndex() const {

			return(audioDecoder->getAudioStreamIndex() + audioStreamOffset);
		}

		virtual void setVideoStreamIndex(int idx) {

			videoDecoder->setVideoStreamIndex(idx);
		}

		virtual void setAudioStreamIndex(int idx) {

			audioDecoder->setAudioStreamIndex(idx - audioStreamOffset);
		}

		virtual int getNrStreams() const {

			return(videoDecoder->getNrStreams() + audioDecoder->getNrStreams());
		}

		virtual AVStream *getVideoStream() const
		{
			return(videoDecoder->getVideoStream());
		}

		virtual const AVCodec *getVideoCodec() const 
		{
			return(videoDecoder->getVideoCodec());
		}

		virtual AVCodecContext *getVideoCodecContext() const 
		{
			return(videoDecoder->getVideoCodecContext());
		}

		virtual AVStream *getAudioStream() const
		{
			return(audioDecoder->getAudioStream());
		}

		virtual const AVCodec *getAudioCodec() const
		{
			return(audioDecoder->getAudioCodec());
		}

		virtual AVCodecContext *getAudioCodecContext() const
		{
			return(audioDecoder->getAudioCodecContext());
		}

		virtual bool hasVideo() const 
		{
			return(videoDecoder->hasVideo());
		}

		virtual bool hasAudio() const 
		{
			return(audioDecoder->hasAudio());
		}

		DashVideoDecoder() {
			
			audioStreamOffset = 0;
			videoDecoder = new VideoDecoder();
			audioDecoder = new VideoDecoder();

			av_init_packet(&videoPacket);
			av_init_packet(&audioPacket);
			videoPacket.data = NULL;
			videoPacket.size = 0;
			audioPacket.data = NULL;
			audioPacket.size = 0;
		}

		virtual ~DashVideoDecoder() {

			close();

			delete videoDecoder;
			delete audioDecoder;
		}

		virtual void close()
		{
			videoDecoder->close();
			audioDecoder->close();

			if(videoPacket.data != NULL) {

				av_packet_unref(&videoPacket);
				videoPacket.data = NULL;
				videoPacket.size = 0;
			}

			if(audioPacket.data != NULL) {

				av_packet_unref(&audioPacket);
				audioPacket.data = NULL;
				audioPacket.size = 0;
			}

			av_init_packet(&videoPacket);
			av_init_packet(&audioPacket);
		}

		virtual void open(OpenVideoArgs ^args, System::Threading::CancellationToken ^token) 
		{
			if(args->VideoLocation != nullptr) {

				videoDecoder->open(args, token);

				audioStreamOffset = videoDecoder->getNrStreams();

			} else {

				audioStreamOffset = 0;
			}

			if(args->AudioLocation != nullptr)
			{							
				audioDecoder->open(gcnew OpenVideoArgs(args->AudioLocation,args->AudioType), token);						
			} 
		}

		int64_t getSizeBytes() const {

			return(videoDecoder->getSizeBytes() + audioDecoder->getSizeBytes());
		}

		double getFrameRate() const {

			return videoDecoder->getFrameRate();
		}

		virtual int getAudioSamplesPerSecond() const {

			return(audioDecoder->getAudioSamplesPerSecond());
		}

		virtual int getAudioBytesPerSample() const {

			return(audioDecoder->getAudioBytesPerSample());
		}

		virtual int getAudioNrChannels() const {

			return(audioDecoder->getAudioNrChannels());
		}

		virtual int getAudioBytesPerSecond() const {

			return(audioDecoder->getAudioBytesPerSecond());
		}

		virtual double getDurationSeconds() const {

			return(Math::Max(videoDecoder->getDurationSeconds(), audioDecoder->getDurationSeconds()));
		}

		virtual AVPixelFormat getOutputPixelFormat() const {

			return(videoDecoder->getOutputPixelFormat());
		}

		virtual AVSampleFormat getOutputSampleFormat() const {

			return(audioDecoder->getOutputSampleFormat());
		}

		virtual int getOutputSampleRate() const {

			return(audioDecoder->getOutputSampleRate());
		}

		virtual int64_t getOutputChannelLayout() const 
		{
			return(audioDecoder->getOutputChannelLayout());
		}

		virtual int getOutputNrChannels() const 
		{
			return(audioDecoder->getOutputNrChannels());
		}

		virtual int getOutputBytesPerSample() const 
		{
			return(audioDecoder->getOutputBytesPerSample());
		}

		virtual FilterGraph *getVideoFilterGraph() const 
		{
			return(videoDecoder->getVideoFilterGraph());
		}

		virtual FilterGraph *getAudioFilterGraph() const 
		{
			return(audioDecoder->getAudioFilterGraph());
		}
	
		// reads a frame from the input stream and places it into a packet
		virtual ReadFrameResult readFrame(AVPacket *packet)
		{
			VideoDecoder::ReadFrameResult result;

			// mux video and audio packets				

			if(videoPacket.data == NULL && audioPacket.data == NULL) {

				if((result = readVideoFrame(videoPacket)) != VideoDecoder::ReadFrameResult::OK) {
					
					av_packet_ref(packet, &videoPacket);
					return result;
				}
				if((result = readAudioFrame(audioPacket)) != VideoDecoder::ReadFrameResult::OK) {
					
					av_packet_ref(packet, &audioPacket);
					return result;
				}
			
			} else {

				if(videoDts <= audioDts) {
				
					if((result = readVideoFrame(videoPacket)) != VideoDecoder::ReadFrameResult::OK) {
						
						av_packet_ref(packet, &videoPacket);
						return result;
					}
				
				} else {
										
					if((result = readAudioFrame(audioPacket)) != VideoDecoder::ReadFrameResult::OK) {
						
						av_packet_ref(packet, &audioPacket);
						return result;
					}					
				}
			
			}
						
			if(videoDts <= audioDts) {

				av_packet_ref(packet, &videoPacket);
						
			} else {

				av_packet_ref(packet, &audioPacket);		
			}
				
			return(result);

		}

		virtual int decodeVideoFrame(AVFrame *picture, int *got_picture_ptr, const AVPacket *avpkt)
		{
			return videoDecoder->decodeVideoFrame(picture,got_picture_ptr, avpkt);
		}

		// returns number of bytes decoded
		virtual int decodeAudioFrame(AVFrame *audio, int *got_audio_ptr, const AVPacket *avpkt) 
		{
			return audioDecoder->decodeAudioFrame(audio, got_audio_ptr, avpkt);
		}

		virtual bool seek(double posSeconds, int flags = AVSEEK_FLAG_BACKWARD) 
		{
			bool result = videoDecoder->seek(posSeconds, flags);
			if(result == false) return(result);
	
			// Seek the audio stream to the current position of the video stream.
			// Seeking in the video stream can be non-exact due to keyframes.
			readVideoFrame(videoPacket);

			result = audioDecoder->seek(videoDts);
			if(result == false) return(result);

			readAudioFrame(audioPacket);
		
			return true;
		}
		
		virtual int getWidth() const 
		{
			return(videoDecoder->getWidth());
		}

		virtual int getHeight() const 
		{
			return(videoDecoder->getHeight());
		}

		virtual void setVideoOutputFormat(AVPixelFormat pixelFormat, int width, int height, 
			SamplingMode sampling) 
		{
			videoDecoder->setVideoOutputFormat(pixelFormat,width,height,sampling);
		}
	
		virtual void convertVideoFrame(AVFrame *input, AVFrame *output)
		{
			videoDecoder->convertVideoFrame(input, output);
		}
		
		virtual void setAudioOutputFormat(int sampleRate = 44100, int64_t channelLayout = AV_CH_LAYOUT_STEREO, 
			AVSampleFormat sampleFormat = AV_SAMPLE_FMT_S16)
		{
			audioDecoder->setAudioOutputFormat(sampleRate, channelLayout, sampleFormat);
		}
	
		virtual int convertAudioFrame(AVFrame *input, AVFrame *output) 
		{
			return audioDecoder->convertAudioFrame(input, output);
		}
	
		virtual bool isClosed() const {

			return(videoDecoder->isClosed() && audioDecoder->isClosed());
		}

		virtual MediaType getMediaType() const {

			if(videoDecoder->getMediaType() == MediaType::VIDEO_MEDIA)
			{				
				return(MediaType::VIDEO_MEDIA);
				
			} else {

				return(audioDecoder->getMediaType());
			}
			
		}

		SeekMode getSeekMode() const
		{
			return(videoDecoder->getSeekMode());
		}
	};

}