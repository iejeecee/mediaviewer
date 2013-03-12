#pragma once
//http://msdn.microsoft.com/en-us/library/windows/desktop/ms804968.aspx
#include "util.h"

namespace imageviewer {

using namespace System::Threading;
namespace DS = Microsoft::DirectX::DirectSound;

public ref class StreamingAudioBuffer
{
public:

	enum class AudioState {

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
	AudioState audioState;
		
	array<AutoResetEvent ^> ^bufferEvents;

	array<unsigned char> ^silence;

	void releaseResources() {

		if(audioBuffer != nullptr) {

			delete audioBuffer;
			audioBuffer = nullptr;
		}
	}

	double pts;
	int ptsPos;
	int prevPtsPos;
	int prevPlayPos;
	int playLoops;
	int ptsLoops;

public: 

	StreamingAudioBuffer(Windows::Forms::Control ^owner)
	{
		device = nullptr;

		try {

			device = gcnew DS::Device();
			device->SetCooperativeLevel(owner, DS::CooperativeLevel::Priority);	

		} catch (DS::SoundException ^exception){

			MessageBox::Show("Error initializing Direct Sound: " + exception->Message, "Direct Sound Error");
			Util::DebugOut("Error Code:" + exception->ErrorCode);
			Util::DebugOut("Error String:" + exception->ErrorString);
			Util::DebugOut("Message:" + exception->Message);
			Util::DebugOut("StackTrace:" + exception->StackTrace);
		}

		audioBuffer = nullptr;
		volume = 1;
		muted = false;

		pts = 0;
		offsetBytes = 0;
		ptsPos = 0;
		prevPtsPos = 0;
		playLoops = 0;
	    ptsLoops = 0;
	}

	~StreamingAudioBuffer() {

		releaseResources();

		if(device != nullptr) {

			delete device;
		}
	}

	void startPlayAfterNextWrite() {

		audioState = AudioState::START_PLAY_AFTER_NEXT_WRITE;
	}

	void flush() {

		if(audioBuffer != nullptr) {

			audioBuffer->Stop();
			audioBuffer->SetCurrentPosition(0);				
			audioBuffer->Write(0, silence, DS::LockFlag::None);
		}

		offsetBytes = 0;
		prevPlayPos = 0;

		ptsPos = 0;
		prevPtsPos = 0;
		playLoops = 0;
	    ptsLoops = 0;

		audioState = AudioState::START_PLAY_AFTER_NEXT_WRITE;
	}

	void stop() {

		if(audioBuffer != nullptr) {
			audioBuffer->Stop();
		}

		audioState = AudioState::STOPPED;
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

	property int Frequency {

		void set(int frequency) {

			audioBuffer->Frequency = frequency;
		}

		int get() {

			return(audioBuffer->Frequency);
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

	void initialize(int samplesPerSecond, int bytesPerSample, int nrChannels, 
		int bufferSizeBytes) 
	{
	
		try {

			releaseResources();

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
			desc->ControlFrequency = true;

			silence = gcnew array<unsigned char>(bufferSizeBytes);
			cli::array<unsigned char>::Clear(silence, 0, silence->Length);

			audioBuffer = gcnew DS::SecondaryBuffer(desc, device);

			Volume = volume;
			offsetBytes = 0;
			prevPlayPos = 0;
			ptsPos = 0;
			prevPtsPos = 0;
			playLoops = 0;
			ptsLoops = 0;

		} catch (DS::SoundException ^exception){

			MessageBox::Show("Error initializing Direct Sound: " + exception->Message, "Direct Sound Error");
			Util::DebugOut("Error Code:" + exception->ErrorCode);
			Util::DebugOut("Error String:" + exception->ErrorString);
			Util::DebugOut("Message:" + exception->Message);
			Util::DebugOut("StackTrace:" + exception->StackTrace);

		} catch (Exception ^e) {

			Util::DebugOut(e->Message);
		}
	}

	double getAudioClock() {

		// audioclock is: pts of last frame plus the
		// difference between playpos and the write position of the last frame in bytes
		// divided by bytespersecond.
		if(audioBuffer == nullptr) return(0);

		int playPos = audioBuffer->PlayPosition;

		if(ptsPos < prevPtsPos) {

			ptsLoops++;
			//Util::DebugOut("ptsLoops" + ptsLoops.ToString());
		}

		if(playPos < prevPlayPos) {

			playLoops++;
			//Util::DebugOut("playLoops" + playLoops.ToString());
		}

		__int64 totalPlayPos = bufferSizeBytes * playLoops + playPos;
		__int64 totalPtsPos = bufferSizeBytes * ptsLoops + ptsPos;

		int bytesPerSecond = samplesPerSecond * bytesPerSample * nrChannels;

		double seconds = (totalPlayPos - totalPtsPos) / double(bytesPerSecond);
	
		double time = pts + seconds;

		prevPlayPos = playPos;
		prevPtsPos = ptsPos;
		
		return(time);
	}

	void write(VideoLib::AudioFrame ^frame) {

		if(audioBuffer == nullptr || frame->Length == 0) return;

		// store pts for this frame and the byte offset at which this frame is
		// written
		pts = frame->Pts;		
		ptsPos = offsetBytes;

		int playPos, writePos;
		audioBuffer->GetCurrentPosition(playPos, writePos);

		if(playPos <= offsetBytes && offsetBytes < writePos) { 
			
			Util::DebugOut("ERROR playpos:" + playPos.ToString() + " offset:" + offsetBytes.ToString() + " writePos:" + writePos.ToString() + " dataSize:" + frame->Length.ToString());
			offsetBytes = writePos;
		} 

		audioBuffer->Write(offsetBytes, frame->Stream, frame->Length, DS::LockFlag::None);

		offsetBytes = (offsetBytes + frame->Length) % bufferSizeBytes;

		if(audioState == AudioState::START_PLAY_AFTER_NEXT_WRITE) {

			audioBuffer->Play(0, DS::BufferPlayFlags::Looping);
			audioState = AudioState::PLAYING;
		}

	}


	

};



}