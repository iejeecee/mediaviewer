#pragma once

#include "Frame.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace Microsoft::DirectX::Direct3D;
using namespace Microsoft::DirectX;
using namespace System::IO;

namespace VideoLib {

	public ref class AudioFrame : public Frame
	{
	private:
	
		MemoryStream ^stream;
		array<unsigned char> ^data;
		int length;
	public:


		AudioFrame(int size) : Frame(FrameType::AUDIO) {

			data = gcnew array<unsigned char>(size);
			stream = gcnew MemoryStream(data);
			length = 0;
			
		}

		property MemoryStream ^Stream {

			MemoryStream ^get() {

				return(stream);
			}

		}

		property array<unsigned char> ^Data {

			array<unsigned char> ^get() {

				return(data);
			}

		}

		property int Length {

			int get() {

				return(length);
			}

		}

		void copyFrameData(BYTE *dataPtr, int length)
		{
			this->length = length;
			Marshal::Copy(IntPtr(dataPtr), data, 0, length);
			
		}
	};
}