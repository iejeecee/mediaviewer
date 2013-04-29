#pragma once
//http://msdn.microsoft.com/en-us/library/windows/desktop/ms804968.aspx
#include "util.h"

namespace imageviewer {

using namespace System::Threading;
using namespace SharpDX::DirectSound;
using namespace SharpDX::Multimedia;

#define DSBVOLUME_MIN               -10000
#define DSBVOLUME_MAX               0

public ref class StreamingAudioBuffer
{
public:

	enum class AudioState {

		START_PLAY_AFTER_NEXT_WRITE,
		PLAYING,
		STOPPED
	};

private:

	static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

	Windows::Forms::Control ^owner;

	DirectSound ^directSound;
	SecondarySoundBuffer ^audioBuffer;
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
		directSound = nullptr;
		this->owner = owner;

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

		if(directSound != nullptr) {

			delete directSound;
		}
	}

	void startPlayAfterNextWrite() {

		audioState = AudioState::START_PLAY_AFTER_NEXT_WRITE;
	}

	void flush() {

		if(audioBuffer != nullptr) {

			audioBuffer->Stop();
			audioBuffer->CurrentPosition = 0;			
			audioBuffer->Write(silence, 0, LockFlags::None);
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

	property int SamplesPerSecond {

		void set(int samplesPerSecond) {

			audioBuffer->Frequency = samplesPerSecond;
		}

		int get() {

			return(audioBuffer->Frequency);
		}
	}

	property double Volume {

		void set(double volume) {

			this->volume = volume;

			if(audioBuffer != nullptr && muted == false) {

				int min = int(DSBVOLUME_MIN) - int(DSBVOLUME_MIN) / 3;

				audioBuffer->Volume = Util::lerp<int>(volume, min, (int)DSBVOLUME_MAX);;

			} else if(audioBuffer != nullptr && muted == true) {

				audioBuffer->Volume = int(DSBVOLUME_MIN);
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

			if(directSound == nullptr) {

				directSound = gcnew DirectSound();
				directSound->SetCooperativeLevel(owner->Handle, CooperativeLevel::Priority);	
			}

			releaseResources();

			this->bufferSizeBytes = bufferSizeBytes;
			this->bytesPerSample = bytesPerSample;
			this->samplesPerSecond = samplesPerSecond;
			this->nrChannels = nrChannels;

			SoundBufferDescription ^desc = gcnew SoundBufferDescription();
			desc->BufferBytes = bufferSizeBytes;
			desc->Flags = BufferFlags::Defer | BufferFlags::GlobalFocus |
				BufferFlags::ControlVolume | BufferFlags::ControlFrequency |
				BufferFlags::GetCurrentPosition2;
		
			//desc->AlgorithmFor3D = Guid::Empty;
			
			WaveFormat ^format = gcnew WaveFormat(samplesPerSecond,
				bytesPerSample * 8, nrChannels);

			desc->Format = format;
/*
			desc->Format->SampleRate = samplesPerSecond;
			desc->Format->BitsPerSample = bytesPerSample * 8;
			desc->Format->Channels = nrChannels;
			desc->Format->FormatTag = WaveFormatTag::Pcm;
			desc->Format->BlockAlign = (short)(format.Channels * (format.BitsPerSample / 8));
			desc->Format->AverageBytesPerSecond = format.SamplesPerSecond * format.BlockAlign;
*/
		
			/*desc->DeferLocation = true;
			desc->GlobalFocus = true;
			desc->ControlVolume = true;
			desc->CanGetCurrentPosition = true;
			desc->ControlFrequency = true;*/

			silence = gcnew array<unsigned char>(bufferSizeBytes);
			cli::array<unsigned char>::Clear(silence, 0, silence->Length);

			audioBuffer = gcnew SecondarySoundBuffer(directSound, desc);

			Volume = volume;
			offsetBytes = 0;
			prevPlayPos = 0;
			ptsPos = 0;
			prevPtsPos = 0;
			playLoops = 0;
			ptsLoops = 0;

			log->Info("Direct Sound Initialized");

		} catch (SharpDX::SharpDXException ^e){

			log->Error("Error initializing Direct Sound", e);
			MessageBox::Show("Error initializing Direct Sound: " + e->Message, "Direct Sound Error");		

		} catch (Exception ^e) {

			log->Error("Error initializing Direct Sound", e);
		}
	}

	double getAudioClock() {

		// audioclock is: pts of last frame plus the
		// difference between playpos and the write position of the last frame in bytes
		// divided by bytespersecond.
		if(audioBuffer == nullptr) return(0);

		int playPos, writePos; 
		
		audioBuffer->GetCurrentPosition(playPos, writePos);

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
			
			log->Warn("playpos:" + playPos.ToString() + " offset:" + offsetBytes.ToString() + " writePos:" + writePos.ToString() + " dataSize:" + frame->Length.ToString());
			offsetBytes = writePos;
		} 

		audioBuffer->Write(frame->Data, 0, frame->Length, offsetBytes, LockFlags::None);

		offsetBytes = (offsetBytes + frame->Length) % bufferSizeBytes;

		if(audioState == AudioState::START_PLAY_AFTER_NEXT_WRITE) {

			audioBuffer->Play(0, PlayFlags::Looping);
			audioState = AudioState::PLAYING;
		}

	}


	

};



}