#pragma once
#include "..\Video\Video.h"

namespace VideoLib {

	public ref class Frame 
	{
	public:

		enum class FrameType {

			AUDIO,
			VIDEO
		};

	protected:

		AVFrame *avFrame;

		double pts,dts;
		FrameType frameType;

	public:

		void setFrameDefaults() {

			av_frame_unref(AVLibFrameData);			
		}

		property AVFrame *AVLibFrameData
		{
			AVFrame *get() {

				return(avFrame);
			}			
		}

		Frame(FrameType frameType) {

			avFrame = av_frame_alloc();
			if(avFrame == NULL) {

				throw gcnew VideoLibException("Cannot allocate Frame");
			}

			this->frameType = frameType;
			pts = 0;
			dts = 0;
		}

		!Frame() {

			if(avFrame != NULL) {

				av_free(avFrame);
				avFrame = NULL;
			}
		}

		~Frame() {

			this->!Frame();
		}

		// presentation timestamp of the current frame in seconds, shifted by stream start_time
		property double Pts {

			void set(double pts) {
				this->pts = pts;
			}

			double get() {
				return(pts);
			}
		}

		// decoding timestamp of the current frame in seconds, shifted by stream start_time
		property double Dts {

			void set(double dts) {
				this->dts = dts;
			}

			double get() {
				return(dts);
			}
		}

		property long FramePts {

			long get()
			{ 
				return(avFrame->pts);
			}
		}
		
		property FrameType FrameTypeP
		{
			FrameType get() {

				return(frameType);
			}

		}

	};
}