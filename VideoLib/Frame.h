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

		double pts;
		FrameType frameType;

	public:


		Frame(FrameType frameType) {

			this->frameType = frameType;
			pts = 0;
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