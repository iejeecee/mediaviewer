#pragma once
//http://msdn.microsoft.com/en-us/library/windows/desktop/ms804968.aspx
#include "util.h"

namespace imageviewer {

using namespace System::Threading;
namespace DS = Microsoft::DirectX::DirectSound;

public ref class StreamingAudioBuffer
{
public:

	enum class StatusMode {

		START_PLAY_AFTER_NEXT_WRITE,
		PLAYING,
		STOPPED
	};

private:

	DS::Device ^device;
	DS::SecondaryBuffer ^audioBuffer;
	int offsetBytes;
	int bufferSizeBytes;

	int bytesPerSample;
	int samplesPerSecond;
	int nrChannels;

	double volume;
	bool muted;
	StatusMode status;
	

	double timeOffset;
	bool setTimeOffset;
	int prevPlayPos;
	int loops;
	array<AutoResetEvent ^> ^bufferEvents;

	

public: 

	void startPlayAfterNextWrite() {

		status = StatusMode::START_PLAY_AFTER_NEXT_WRITE;
	}

	void flush() {

		if(audioBuffer != nullptr) {
			audioBuffer->Stop();
			audioBuffer->SetCurrentPosition(0);	
		}

		loops = 0;
		offsetBytes = 0;
		prevPlayPos = 0;

		setTimeOffset = true;
		status = StatusMode::START_PLAY_AFTER_NEXT_WRITE;
	}

	void stop() {

		status = StatusMode::STOPPED;

		if(audioBuffer != nullptr) {
			audioBuffer->Stop();
		}
	}

	property bool Muted {

		void set(bool muted) {
			
			this->muted = muted;
			Volume = volume;
			
		}

		bool get() {

			return(muted);
		}
	}

	property double Volume {

		void set(double volume) {

			this->volume = volume;

			if(audioBuffer != nullptr && muted == false) {

				int min = int(DS::Volume::Min) - int(DS::Volume::Min) / 3;

				audioBuffer->Volume = Util::lerp<int>(volume, min, (int)DS::Volume::Max);;

			} else if(audioBuffer != nullptr && muted == true) {

				audioBuffer->Volume = int(DS::Volume::Min);
			}
		}

		double get() {

			return(volume);
		}

	}

	StreamingAudioBuffer(Windows::Forms::Control ^owner)
	{
			try {

				device = gcnew DS::Device();
				device->SetCooperativeLevel(owner, DS::CooperativeLevel::Priority);	

			} catch (DS::SoundException ^exception){


				Util::DebugOut("Error Code:" + exception->ErrorCode);
				Util::DebugOut("Error String:" + exception->ErrorString);
				Util::DebugOut("Message:" + exception->Message);
				Util::DebugOut("StackTrace:" + exception->StackTrace);
			}

			audioBuffer = nullptr;
			volume = 1;
			muted = false;
			timeOffset = 0;
			setTimeOffset = true;
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

			Volume = volume;
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

		if(audioBuffer == nullptr) return(0);

		int playPos = audioBuffer->PlayPosition;

		if(playPos < prevPlayPos) {

			loops++;
		}

		__int64 bytesPlayed = bufferSizeBytes * loops + playPos;
		int bytesPerSecond = samplesPerSecond * bytesPerSample * nrChannels;
		double time = timeOffset + bytesPlayed / double(bytesPerSecond);

		prevPlayPos = playPos;

		return(time);
	}

	void write(VideoLib::AudioFrame ^frame) {//Stream ^data, int dataSizeBytes) {

		if(frame->Length == 0) return;

		int playPos, writePos;
		audioBuffer->GetCurrentPosition(playPos, writePos);

		if(playPos <= offsetBytes && offsetBytes < writePos) { 
			
			Util::DebugOut("ERROR playpos:" + playPos.ToString() + " offset:" + offsetBytes.ToString() + " writePos:" + writePos.ToString() + " dataSize:" + frame->Length.ToString());
			offsetBytes = writePos;
		} 

		audioBuffer->Write(offsetBytes, frame->Stream, frame->Length, DS::LockFlag::None);

		offsetBytes = (offsetBytes + frame->Length) % bufferSizeBytes;

		if(status == StatusMode::START_PLAY_AFTER_NEXT_WRITE) {

			if(setTimeOffset == true) {

				timeOffset = frame->Pts;
				setTimeOffset = false;
			}

			audioBuffer->Play(0, DS::BufferPlayFlags::Looping);
			status = StatusMode::PLAYING;
		}

	}

	

};



}