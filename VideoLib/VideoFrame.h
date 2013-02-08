#pragma once

using namespace Microsoft::DirectX::Direct3D;

namespace VideoLib {

	public ref class VideoFrame
	{
	private:

		double pts;
		Surface ^frame;

	public:

		property double Pts {

			void set(double pts) {
				this->pts = pts;
			}

			double get() {
				return(pts);
			}
		}

		property Surface ^Frame {

			void set(Surface ^frame) {
				this->frame = frame;
			}

			Surface ^get() {
				return(frame);
			}
		}

		VideoFrame(Device ^device, int width, int height, Format pixelFormat) {

			frame = device->CreateOffscreenPlainSurface(width, height, pixelFormat, 
				Pool::Default);
			pts = 0;
		}

		~VideoFrame() {

			if(frame != nullptr) {

				delete frame;
			}
		}
	};
}