#pragma once
//http://msdn.microsoft.com/en-us/library/windows/desktop/ms804968.aspx
#include "util.h"

namespace imageviewer {

namespace DS = Microsoft::DirectX::DirectSound;

public ref class StreamingAudioBuffer
{

private:

	DS::Device ^device;
	DS::SecondaryBuffer ^audioBuffer;
	int offsetBytes;
	int bufferSizeBytes;
	int oneSecondSizeBytes;

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

	}

	void initialize(int samplesPerSecond, int bytesPerSample, int nrChannels, 
		int bufferSizeBytes) 
	{
	
		this->bufferSizeBytes = bufferSizeBytes;

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
		oneSecondSizeBytes = samplesPerSecond * bytesPerSample;

	}

	void write(Stream ^data, int dataSizeBytes) {

		int playPos, writePos;
		audioBuffer->GetCurrentPosition(playPos, writePos);
		data->Position = 0;

		audioBuffer->Write(offsetBytes, data, dataSizeBytes, DS::LockFlag::None);

		if(playPos <= offsetBytes && offsetBytes <= writePos) { 

			Util::DebugOut("ERROR playpos:" + playPos.ToString() + " offset:" + offsetBytes.ToString() + " writePos:" + writePos.ToString() + " dataSize:" + dataSizeBytes.ToString());
		}		

		offsetBytes = (offsetBytes + dataSizeBytes) % bufferSizeBytes;

		if(offsetBytes >= oneSecondSizeBytes && audioBuffer->Status.Playing == false) {

			audioBuffer->Play(0, DS::BufferPlayFlags::Looping);
		}

	
	}

	

};



}