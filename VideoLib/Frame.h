#pragma once

namespace VideoLib {

	public ref class Frame 
	{
	public:

		enum class FrameType {

			AUDIO,
			VIDEO
		};

	private:

		AVFrame *avFrame;

		double pts;
		FrameType frameType;

	public:

		property AVFrame *AVLibFrameData
		{
			AVFrame *get() {

				return(avFrame);
			}
		}

		Frame(FrameType frameType) {

			avFrame = avcodec_alloc_frame();

			this->frameType = frameType;
			pts = 0;
		}

		~Frame() {

			av_free(avFrame);
		}

		property double Pts {

			void set(double pts) {
				this->pts = pts;
			}

			double get() {
				return(pts);
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