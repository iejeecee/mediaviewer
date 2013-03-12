#pragma once

#include "util.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for VideoDebugForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class VideoDebugForm : public System::Windows::Forms::Form
	{
	public:
		VideoDebugForm(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			clear();
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~VideoDebugForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Label^  audioFrameSizeLabel;
	protected: 
	private: System::Windows::Forms::Label^  label15;
	private: System::Windows::Forms::Label^  actualAudioDelayLabel;
	private: System::Windows::Forms::Label^  actualVideoDelayLabel;
	private: System::Windows::Forms::Label^  label14;
	private: System::Windows::Forms::Label^  label13;
	private: System::Windows::Forms::Label^  audioQueueSizeLabel;
	private: System::Windows::Forms::Label^  videoQueueSizeLabel;
	private: System::Windows::Forms::Label^  label12;
	private: System::Windows::Forms::Label^  label11;
	private: System::Windows::Forms::Label^  audioDroppedLabel;
	private: System::Windows::Forms::Label^  videoDroppedLabel;
	private: System::Windows::Forms::Label^  label8;
	private: System::Windows::Forms::Label^  label7;
	private: System::Windows::Forms::Label^  audioFramesLabel;
	private: System::Windows::Forms::Label^  videoFramesLabel;
	private: System::Windows::Forms::Label^  label10;
	private: System::Windows::Forms::Label^  label9;
	private: System::Windows::Forms::Label^  audioDelayLabel;
	private: System::Windows::Forms::Label^  videoDelayLabel;
	private: System::Windows::Forms::Label^  label5;
	private: System::Windows::Forms::Label^  label6;
	private: System::Windows::Forms::Label^  audioSyncLabel;
	private: System::Windows::Forms::Label^  videoSyncLabel;
	private: System::Windows::Forms::Label^  label4;
	private: System::Windows::Forms::Label^  label3;
	private: System::Windows::Forms::Label^  audioQueueLabel;
	private: System::Windows::Forms::Label^  videoQueueLabel;
	private: System::Windows::Forms::Label^  label2;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Label^  audioLengthAdjustLabel;
	private: System::Windows::Forms::Label^  videoSyncMasterLabel;
	private: System::Windows::Forms::Label^  audioSyncMasterLabel;

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->audioFrameSizeLabel = (gcnew System::Windows::Forms::Label());
			this->label15 = (gcnew System::Windows::Forms::Label());
			this->actualAudioDelayLabel = (gcnew System::Windows::Forms::Label());
			this->actualVideoDelayLabel = (gcnew System::Windows::Forms::Label());
			this->label14 = (gcnew System::Windows::Forms::Label());
			this->label13 = (gcnew System::Windows::Forms::Label());
			this->audioQueueSizeLabel = (gcnew System::Windows::Forms::Label());
			this->videoQueueSizeLabel = (gcnew System::Windows::Forms::Label());
			this->label12 = (gcnew System::Windows::Forms::Label());
			this->label11 = (gcnew System::Windows::Forms::Label());
			this->audioDroppedLabel = (gcnew System::Windows::Forms::Label());
			this->videoDroppedLabel = (gcnew System::Windows::Forms::Label());
			this->label8 = (gcnew System::Windows::Forms::Label());
			this->label7 = (gcnew System::Windows::Forms::Label());
			this->audioFramesLabel = (gcnew System::Windows::Forms::Label());
			this->videoFramesLabel = (gcnew System::Windows::Forms::Label());
			this->label10 = (gcnew System::Windows::Forms::Label());
			this->label9 = (gcnew System::Windows::Forms::Label());
			this->audioDelayLabel = (gcnew System::Windows::Forms::Label());
			this->videoDelayLabel = (gcnew System::Windows::Forms::Label());
			this->label5 = (gcnew System::Windows::Forms::Label());
			this->label6 = (gcnew System::Windows::Forms::Label());
			this->audioSyncLabel = (gcnew System::Windows::Forms::Label());
			this->videoSyncLabel = (gcnew System::Windows::Forms::Label());
			this->label4 = (gcnew System::Windows::Forms::Label());
			this->label3 = (gcnew System::Windows::Forms::Label());
			this->audioQueueLabel = (gcnew System::Windows::Forms::Label());
			this->videoQueueLabel = (gcnew System::Windows::Forms::Label());
			this->label2 = (gcnew System::Windows::Forms::Label());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->audioLengthAdjustLabel = (gcnew System::Windows::Forms::Label());
			this->videoSyncMasterLabel = (gcnew System::Windows::Forms::Label());
			this->audioSyncMasterLabel = (gcnew System::Windows::Forms::Label());
			this->SuspendLayout();
			// 
			// audioFrameSizeLabel
			// 
			this->audioFrameSizeLabel->AutoSize = true;
			this->audioFrameSizeLabel->Location = System::Drawing::Point(512, 196);
			this->audioFrameSizeLabel->Name = L"audioFrameSizeLabel";
			this->audioFrameSizeLabel->Size = System::Drawing::Size(18, 20);
			this->audioFrameSizeLabel->TabIndex = 74;
			this->audioFrameSizeLabel->Text = L"0";
			// 
			// label15
			// 
			this->label15->AutoSize = true;
			this->label15->Location = System::Drawing::Point(412, 196);
			this->label15->Name = L"label15";
			this->label15->Size = System::Drawing::Size(94, 20);
			this->label15->TabIndex = 73;
			this->label15->Text = L"Frame Size:";
			// 
			// actualAudioDelayLabel
			// 
			this->actualAudioDelayLabel->AutoSize = true;
			this->actualAudioDelayLabel->Location = System::Drawing::Point(300, 196);
			this->actualAudioDelayLabel->Name = L"actualAudioDelayLabel";
			this->actualAudioDelayLabel->Size = System::Drawing::Size(18, 20);
			this->actualAudioDelayLabel->TabIndex = 72;
			this->actualAudioDelayLabel->Text = L"0";
			// 
			// actualVideoDelayLabel
			// 
			this->actualVideoDelayLabel->AutoSize = true;
			this->actualVideoDelayLabel->Location = System::Drawing::Point(300, 176);
			this->actualVideoDelayLabel->Name = L"actualVideoDelayLabel";
			this->actualVideoDelayLabel->Size = System::Drawing::Size(18, 20);
			this->actualVideoDelayLabel->TabIndex = 71;
			this->actualVideoDelayLabel->Text = L"0";
			// 
			// label14
			// 
			this->label14->AutoSize = true;
			this->label14->Location = System::Drawing::Point(192, 176);
			this->label14->Name = L"label14";
			this->label14->Size = System::Drawing::Size(102, 20);
			this->label14->TabIndex = 70;
			this->label14->Text = L"Actual Delay:";
			// 
			// label13
			// 
			this->label13->AutoSize = true;
			this->label13->Location = System::Drawing::Point(192, 196);
			this->label13->Name = L"label13";
			this->label13->Size = System::Drawing::Size(102, 20);
			this->label13->TabIndex = 69;
			this->label13->Text = L"Actual Delay:";
			// 
			// audioQueueSizeLabel
			// 
			this->audioQueueSizeLabel->AutoSize = true;
			this->audioQueueSizeLabel->Location = System::Drawing::Point(309, 87);
			this->audioQueueSizeLabel->Name = L"audioQueueSizeLabel";
			this->audioQueueSizeLabel->Size = System::Drawing::Size(18, 20);
			this->audioQueueSizeLabel->TabIndex = 68;
			this->audioQueueSizeLabel->Text = L"0";
			// 
			// videoQueueSizeLabel
			// 
			this->videoQueueSizeLabel->AutoSize = true;
			this->videoQueueSizeLabel->Location = System::Drawing::Point(309, 67);
			this->videoQueueSizeLabel->Name = L"videoQueueSizeLabel";
			this->videoQueueSizeLabel->Size = System::Drawing::Size(18, 20);
			this->videoQueueSizeLabel->TabIndex = 67;
			this->videoQueueSizeLabel->Text = L"0";
			// 
			// label12
			// 
			this->label12->AutoSize = true;
			this->label12->Location = System::Drawing::Point(259, 87);
			this->label12->Name = L"label12";
			this->label12->Size = System::Drawing::Size(44, 20);
			this->label12->TabIndex = 66;
			this->label12->Text = L"Size:";
			// 
			// label11
			// 
			this->label11->AutoSize = true;
			this->label11->Location = System::Drawing::Point(259, 67);
			this->label11->Name = L"label11";
			this->label11->Size = System::Drawing::Size(44, 20);
			this->label11->TabIndex = 65;
			this->label11->Text = L"Size:";
			// 
			// audioDroppedLabel
			// 
			this->audioDroppedLabel->AutoSize = true;
			this->audioDroppedLabel->Location = System::Drawing::Point(340, 33);
			this->audioDroppedLabel->Name = L"audioDroppedLabel";
			this->audioDroppedLabel->Size = System::Drawing::Size(18, 20);
			this->audioDroppedLabel->TabIndex = 64;
			this->audioDroppedLabel->Text = L"0";
			// 
			// videoDroppedLabel
			// 
			this->videoDroppedLabel->AutoSize = true;
			this->videoDroppedLabel->Location = System::Drawing::Point(340, 13);
			this->videoDroppedLabel->Name = L"videoDroppedLabel";
			this->videoDroppedLabel->Size = System::Drawing::Size(18, 20);
			this->videoDroppedLabel->TabIndex = 63;
			this->videoDroppedLabel->Text = L"0";
			// 
			// label8
			// 
			this->label8->AutoSize = true;
			this->label8->Location = System::Drawing::Point(259, 33);
			this->label8->Name = L"label8";
			this->label8->Size = System::Drawing::Size(73, 20);
			this->label8->TabIndex = 62;
			this->label8->Text = L"Sped up:";
			// 
			// label7
			// 
			this->label7->AutoSize = true;
			this->label7->Location = System::Drawing::Point(259, 13);
			this->label7->Name = L"label7";
			this->label7->Size = System::Drawing::Size(75, 20);
			this->label7->TabIndex = 61;
			this->label7->Text = L"Dropped:";
			// 
			// audioFramesLabel
			// 
			this->audioFramesLabel->AutoSize = true;
			this->audioFramesLabel->Location = System::Drawing::Point(123, 33);
			this->audioFramesLabel->Name = L"audioFramesLabel";
			this->audioFramesLabel->Size = System::Drawing::Size(18, 20);
			this->audioFramesLabel->TabIndex = 60;
			this->audioFramesLabel->Text = L"0";
			// 
			// videoFramesLabel
			// 
			this->videoFramesLabel->AutoSize = true;
			this->videoFramesLabel->Location = System::Drawing::Point(123, 13);
			this->videoFramesLabel->Name = L"videoFramesLabel";
			this->videoFramesLabel->Size = System::Drawing::Size(18, 20);
			this->videoFramesLabel->TabIndex = 59;
			this->videoFramesLabel->Text = L"0";
			// 
			// label10
			// 
			this->label10->AutoSize = true;
			this->label10->Location = System::Drawing::Point(7, 33);
			this->label10->Name = L"label10";
			this->label10->Size = System::Drawing::Size(112, 20);
			this->label10->TabIndex = 58;
			this->label10->Text = L"Audio Frames:";
			// 
			// label9
			// 
			this->label9->AutoSize = true;
			this->label9->Location = System::Drawing::Point(7, 13);
			this->label9->Name = L"label9";
			this->label9->Size = System::Drawing::Size(112, 20);
			this->label9->TabIndex = 57;
			this->label9->Text = L"Video Frames:";
			// 
			// audioDelayLabel
			// 
			this->audioDelayLabel->AutoSize = true;
			this->audioDelayLabel->Location = System::Drawing::Point(110, 196);
			this->audioDelayLabel->Name = L"audioDelayLabel";
			this->audioDelayLabel->Size = System::Drawing::Size(18, 20);
			this->audioDelayLabel->TabIndex = 56;
			this->audioDelayLabel->Text = L"0";
			// 
			// videoDelayLabel
			// 
			this->videoDelayLabel->AutoSize = true;
			this->videoDelayLabel->Location = System::Drawing::Point(110, 176);
			this->videoDelayLabel->Name = L"videoDelayLabel";
			this->videoDelayLabel->Size = System::Drawing::Size(18, 20);
			this->videoDelayLabel->TabIndex = 55;
			this->videoDelayLabel->Text = L"0";
			// 
			// label5
			// 
			this->label5->AutoSize = true;
			this->label5->Location = System::Drawing::Point(7, 196);
			this->label5->Name = L"label5";
			this->label5->Size = System::Drawing::Size(98, 20);
			this->label5->TabIndex = 54;
			this->label5->Text = L"Audio Delay:";
			// 
			// label6
			// 
			this->label6->AutoSize = true;
			this->label6->Location = System::Drawing::Point(7, 176);
			this->label6->Name = L"label6";
			this->label6->Size = System::Drawing::Size(98, 20);
			this->label6->TabIndex = 53;
			this->label6->Text = L"Video Delay:";
			// 
			// audioSyncLabel
			// 
			this->audioSyncLabel->AutoSize = true;
			this->audioSyncLabel->Location = System::Drawing::Point(101, 138);
			this->audioSyncLabel->Name = L"audioSyncLabel";
			this->audioSyncLabel->Size = System::Drawing::Size(18, 20);
			this->audioSyncLabel->TabIndex = 52;
			this->audioSyncLabel->Text = L"0";
			// 
			// videoSyncLabel
			// 
			this->videoSyncLabel->AutoSize = true;
			this->videoSyncLabel->Location = System::Drawing::Point(101, 118);
			this->videoSyncLabel->Name = L"videoSyncLabel";
			this->videoSyncLabel->Size = System::Drawing::Size(18, 20);
			this->videoSyncLabel->TabIndex = 51;
			this->videoSyncLabel->Text = L"0";
			// 
			// label4
			// 
			this->label4->AutoSize = true;
			this->label4->Location = System::Drawing::Point(7, 138);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(93, 20);
			this->label4->TabIndex = 50;
			this->label4->Text = L"Audio Sync:";
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(7, 118);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(93, 20);
			this->label3->TabIndex = 49;
			this->label3->Text = L"Video Sync:";
			// 
			// audioQueueLabel
			// 
			this->audioQueueLabel->AutoSize = true;
			this->audioQueueLabel->Location = System::Drawing::Point(110, 87);
			this->audioQueueLabel->Name = L"audioQueueLabel";
			this->audioQueueLabel->Size = System::Drawing::Size(31, 20);
			this->audioQueueLabel->TabIndex = 48;
			this->audioQueueLabel->Text = L"0/0";
			// 
			// videoQueueLabel
			// 
			this->videoQueueLabel->AutoSize = true;
			this->videoQueueLabel->Location = System::Drawing::Point(110, 67);
			this->videoQueueLabel->Name = L"videoQueueLabel";
			this->videoQueueLabel->Size = System::Drawing::Size(31, 20);
			this->videoQueueLabel->TabIndex = 47;
			this->videoQueueLabel->Text = L"0/0";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(7, 87);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(106, 20);
			this->label2->TabIndex = 46;
			this->label2->Text = L"Audio Queue:";
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(7, 67);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(106, 20);
			this->label1->TabIndex = 45;
			this->label1->Text = L"Video Queue:";
			// 
			// audioLengthAdjustLabel
			// 
			this->audioLengthAdjustLabel->AutoSize = true;
			this->audioLengthAdjustLabel->Location = System::Drawing::Point(575, 196);
			this->audioLengthAdjustLabel->Name = L"audioLengthAdjustLabel";
			this->audioLengthAdjustLabel->Size = System::Drawing::Size(18, 20);
			this->audioLengthAdjustLabel->TabIndex = 75;
			this->audioLengthAdjustLabel->Text = L"0";
			// 
			// videoSyncMasterLabel
			// 
			this->videoSyncMasterLabel->AutoSize = true;
			this->videoSyncMasterLabel->Location = System::Drawing::Point(259, 118);
			this->videoSyncMasterLabel->Name = L"videoSyncMasterLabel";
			this->videoSyncMasterLabel->Size = System::Drawing::Size(76, 20);
			this->videoSyncMasterLabel->TabIndex = 76;
			this->videoSyncMasterLabel->Text = L"MASTER";
			// 
			// audioSyncMasterLabel
			// 
			this->audioSyncMasterLabel->AutoSize = true;
			this->audioSyncMasterLabel->Location = System::Drawing::Point(259, 138);
			this->audioSyncMasterLabel->Name = L"audioSyncMasterLabel";
			this->audioSyncMasterLabel->Size = System::Drawing::Size(76, 20);
			this->audioSyncMasterLabel->TabIndex = 77;
			this->audioSyncMasterLabel->Text = L"MASTER";
			// 
			// VideoDebugForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(629, 232);
			this->ControlBox = false;
			this->Controls->Add(this->audioSyncMasterLabel);
			this->Controls->Add(this->videoSyncMasterLabel);
			this->Controls->Add(this->audioLengthAdjustLabel);
			this->Controls->Add(this->audioFrameSizeLabel);
			this->Controls->Add(this->label15);
			this->Controls->Add(this->actualAudioDelayLabel);
			this->Controls->Add(this->actualVideoDelayLabel);
			this->Controls->Add(this->label14);
			this->Controls->Add(this->label13);
			this->Controls->Add(this->audioQueueSizeLabel);
			this->Controls->Add(this->videoQueueSizeLabel);
			this->Controls->Add(this->label12);
			this->Controls->Add(this->label11);
			this->Controls->Add(this->audioDroppedLabel);
			this->Controls->Add(this->videoDroppedLabel);
			this->Controls->Add(this->label8);
			this->Controls->Add(this->label7);
			this->Controls->Add(this->audioFramesLabel);
			this->Controls->Add(this->videoFramesLabel);
			this->Controls->Add(this->label10);
			this->Controls->Add(this->label9);
			this->Controls->Add(this->audioDelayLabel);
			this->Controls->Add(this->videoDelayLabel);
			this->Controls->Add(this->label5);
			this->Controls->Add(this->label6);
			this->Controls->Add(this->audioSyncLabel);
			this->Controls->Add(this->videoSyncLabel);
			this->Controls->Add(this->label4);
			this->Controls->Add(this->label3);
			this->Controls->Add(this->audioQueueLabel);
			this->Controls->Add(this->videoQueueLabel);
			this->Controls->Add(this->label2);
			this->Controls->Add(this->label1);
			this->Name = L"VideoDebugForm";
			this->Text = L"Video Debug";
			this->TopMost = true;
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

private:

	double audioDelay;
	double videoDelay;

	double audioSync;
	double videoSync;

	int audioQueueSize;
	int audioQueueMaxSize;

	int videoQueueSize;
	int videoQueueMaxSize;

	__int64 videoFrames;
	__int64 audioFrames;

	__int64 nrVideoFramesDropped;
	__int64 nrAudioFramesLaggingBehind;

	int videoQueueSizeBytes;
	int audioQueueSizeBytes;

	double actualVideoDelay;
	double actualAudioDelay;
	int audioFrameLength;
	int audioFrameLengthAdjust;

	bool isVideoSyncMaster;
	bool isAudioSyncMaster;

	void updateInvoke() {

		videoFramesLabel->Text = videoFrames.ToString();
		audioFramesLabel->Text = audioFrames.ToString();

		videoDroppedLabel->Text = nrVideoFramesDropped.ToString();
		audioDroppedLabel->Text = nrAudioFramesLaggingBehind.ToString();

		videoQueueLabel->Text = videoQueueSize.ToString() + "/" + videoQueueMaxSize.ToString();
		audioQueueLabel->Text = audioQueueSize.ToString() + "/" + audioQueueMaxSize.ToString();

		videoSyncLabel->Text = videoSync.ToString("F4");
		videoSyncMasterLabel->Visible = IsVideoSyncMaster;
		audioSyncLabel->Text = audioSync.ToString("F4");
		audioSyncMasterLabel->Visible = IsAudioSyncMaster;

		videoDelayLabel->Text = videoDelay.ToString("F4");
		actualVideoDelayLabel->Text = actualVideoDelay.ToString("F4");
		
		audioDelayLabel->Text = audioDelay.ToString("F4");
		actualAudioDelayLabel->Text = actualAudioDelay.ToString("F4");
		audioFrameSizeLabel->Text = audioFrameLength.ToString();
		audioLengthAdjustLabel->Text = 
			audioFrameLengthAdjust > 0 ? "+" + audioFrameLengthAdjust.ToString() :
			audioFrameLengthAdjust.ToString();

		videoQueueSizeLabel->Text = Util::formatSizeBytes(videoQueueSizeBytes);
		audioQueueSizeLabel->Text = Util::formatSizeBytes(audioQueueSizeBytes);
	}

public:

	void update() {

		if(this->Visible == false) return;

		if(this->InvokeRequired == true) {

			this->Invoke(gcnew System::Action(this, &VideoDebugForm::updateInvoke));

		} else {

			updateInvoke();
		}

	}

	void clear() {

		audioDelay = 0;
		videoDelay = 0;

		audioSync = 0;
		videoSync = 0;

		audioQueueSize = 0;
		audioQueueMaxSize = 0;

		videoQueueSize = 0;
		videoQueueMaxSize = 0;

		videoFrames = 0;
		audioFrames = 0;

		nrVideoFramesDropped = 0;
		nrAudioFramesLaggingBehind = 0;

		videoQueueMaxSize = 0;
		audioQueueMaxSize = 0;

		actualVideoDelay = 0;
		actualAudioDelay = 0;
		audioFrameLength = 0;
	}

	property double AudioDelay {

		double get() {

			return(audioDelay);
		}

		void set(double value) {

			audioDelay = value;
		}
	}

	property double VideoDelay {

		double get() {

			return(videoDelay);
		}

		void set(double value) {

			videoDelay = value;
		}
	}

	property double AudioSync {

		double get() {

			return(audioSync);
		}

		void set(double value) {

			audioSync = value;
		}
	}

	property double VideoSync {

		double get() {

			return(videoSync);
		}

		void set(double value) {

			videoSync = value;
		}
	}

	property int AudioQueueSize {

		int get() {

			return(audioQueueSize);
		}

		void set(int value) {

			audioQueueSize = value;
		}
	}

	property int VideoQueueSize {

		int get() {

			return(videoQueueSize);
		}

		void set(int value) {

			videoQueueSize = value;
		}
	}

	property int AudioQueueMaxSize {

		int get() {

			return(audioQueueMaxSize);
		}

		void set(int value) {

			audioQueueMaxSize = value;
		}
	}

	property int VideoQueueMaxSize {

		int get() {

			return(videoQueueMaxSize);
		}

		void set(int value) {

			videoQueueMaxSize = value; 
		}
	}

	property __int64 VideoFrames {

		__int64 get() {

			return(videoFrames);
		}

		void set(__int64 value) {

			videoFrames = value;
		}
	}

	property __int64 AudioFrames {

		__int64 get() {

			return(audioFrames);
		}

		void set(__int64 value) {

			audioFrames = value;
		}
	}

	property __int64 NrVideoFramesDropped {

		__int64 get() {

			return(nrVideoFramesDropped);
		}

		void set(__int64 value) {

			nrVideoFramesDropped = value;
		}
	}

	property __int64 NrAudioFramesLaggingBehind {

		__int64 get() {

			return(nrAudioFramesLaggingBehind);
		}

		void set(__int64 value) {

			nrAudioFramesLaggingBehind = value;
		}
	}

	property int VideoQueueSizeBytes {

		int get() {

			return(videoQueueSizeBytes);
		}

		void set(int value) {

			videoQueueSizeBytes = value;
		}
	}

	property int AudioQueueSizeBytes {

		int get() {

			return(audioQueueSizeBytes);
		}

		void set(int value) {

			audioQueueSizeBytes = value;
		}
	}

	property double ActualVideoDelay {

		double get() {

			return(actualVideoDelay);
		}

		void set(double value) {

			actualVideoDelay = value;
		}
	}

	property double ActualAudioDelay {

		double get() {

			return(actualAudioDelay);
		}

		void set(double value) {

			actualAudioDelay = value;
		}
	}

	property int AudioFrameLength {

		int get() {

			return(audioFrameLength);
		}

		void set(int value) {

			audioFrameLength = value;
		}
	}

	property int AudioFrameLengthAdjust {

		int get() {

			return(audioFrameLengthAdjust);
		}

		void set(int value) {

			audioFrameLengthAdjust = value;
		}
	}

	property bool IsVideoSyncMaster {

		bool get() {

			return(isVideoSyncMaster);
		}

		void set(bool value) {

			isVideoSyncMaster = value;

			if(value == true) {

				isAudioSyncMaster = false;
			}
		}
	}

	property bool IsAudioSyncMaster {

		bool get() {

			return(isAudioSyncMaster);
		}

		void set(bool value) {

			isAudioSyncMaster = value;

			if(value == true) {

				isVideoSyncMaster = false;
			}
		}
	}


	};
}
