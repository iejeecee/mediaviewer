#pragma once
//http://msdn.microsoft.com/en-us/library/windows/desktop/ms804968.aspx
#include "util.h"

namespace imageviewer {

using namespace System::Threading;
namespace DS = Microsoft::DirectX::DirectSound;

public ref class StreamingAudioBuffer
{

private:

	DS::Device ^device;
	DS::SecondaryBuffer ^audioBuffer;
	int offsetBytes;
	int bufferSizeBytes;

	int bytesPerSample;
	int samplesPerSecond;
	int nrChannels;

	int prevPlayPos;
	int loops;
	array<AutoResetEvent ^> ^bufferEvents;

public: 

	StreamingAudioBuffer(Windows::Forms::Control ^owner)
	{
			try {
				device = gcnew DS::Device();
				device->SetCooperativeLevel(owner, DS::CooperativeLevel::Priority);

				//DS::SecondaryBuffer ^buffer = gcnew DS::SecondaryBuffer(
				//DS::BufferDescription ^desc = gcnew DS::BufferDescription();
				//desc->
			

			} catch (DS::SoundException ^exception){

				Util::DebugOut("Error Code:" + exception->ErrorCode);
				Util::DebugOut("Error String:" + exception->ErrorString);
				Util::DebugOut("Message:" + exception->Message);
				Util::DebugOut("StackTrace:" + exception->StackTrace);
			}

			audioBuffer = nullptr;
	}

	void initialize(int samplesPerSecond, int bytesPerSample, int nrChannels, 
		int bufferSizeBytes) 
	{
	
		try {

			this->bufferSizeBytes = bufferSizeBytes;
			this->bytesPerSample = bytesPerSample;
			this->samplesPerSecond = samplesPerSecond;
			this->nrChannels = nrChannels;

			DS::WaveFormat format;

			format.SamplesPerSecond = samplesPerSecond;
			format.BitsPerSample = bytesPerSample * 8;
			format.Channels = nrChannels;
			format.FormatTag = DS::WaveFormatTag::Pcm;
			format.BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));
			format.AverageBytesPerSecond = format.SamplesPerSecond * format.BlockAlign;

			DS::BufferDescription ^desc = gcnew DS::BufferDescription(format);
			desc->BufferBytes = bufferSizeBytes;
			desc->DeferLocation = true;
			desc->GlobalFocus = true;
			desc->ControlVolume = true;
			desc->CanGetCurrentPosition = true;
			
			audioBuffer = gcnew DS::SecondaryBuffer(desc, device);

			offsetBytes = 0;
			loops = 0;
			prevPlayPos = 0;

		} catch (DS::SoundException ^exception){

			Util::DebugOut("Error Code:" + exception->ErrorCode);
			Util::DebugOut("Error String:" + exception->ErrorString);
			Util::DebugOut("Message:" + exception->Message);
			Util::DebugOut("StackTrace:" + exception->StackTrace);

		} catch (Exception ^e) {

			Util::DebugOut(e->Message);
		}
	}


	double getAudioClock() {

		int playPos = audioBuffer->PlayPosition;

		if(playPos < prevPlayPos) {

			loops++;
		}

		__int64 bytesPlayed = bufferSizeBytes * loops + playPos;
		int bytesPerSecond = samplesPerSecond * bytesPerSample * nrChannels;
		double time = bytesPlayed / double(bytesPerSecond);

		prevPlayPos = playPos;

		return(time);
	}

	void stop() {

		if(audioBuffer != nullptr) {
			audioBuffer->Stop();
		}
	}

	void write(Stream ^data, int dataSizeBytes) {

		int playPos, writePos;
		audioBuffer->GetCurrentPosition(playPos, writePos);

		audioBuffer->Write(offsetBytes, data, dataSizeBytes, DS::LockFlag::None);

		if(playPos <= offsetBytes && offsetBytes < writePos) { 

			Util::DebugOut("ERROR playpos:" + playPos.ToString() + " offset:" + offsetBytes.ToString() + " writePos:" + writePos.ToString() + " dataSize:" + dataSizeBytes.ToString());
		}		

		offsetBytes = (offsetBytes + dataSizeBytes) % bufferSizeBytes;

		if(audioBuffer->Status.Playing == false) {

			audioBuffer->Play(0, DS::BufferPlayFlags::Looping);
		}

	
	}

	

};



}