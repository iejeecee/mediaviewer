#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace imageviewer {

	/// <summary>
	/// Summary for VideoDebug
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class VideoDebug : public System::Windows::Forms::Form
	{
	public:
		VideoDebug(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
			audioDelay = 0;
			videoDelay = 0;

			audioSync = 0;
			videoSync = 0;

			audioQueue = 0;
			audioQueueSize = 0;

			videoQueue = 0;
			videoQueueSize = 0;

			videoFrames = 0;
			audioFrames = 0;

			videoDropped = 0;
			audioDropped = 0;

			videoQueueSize = 0;
			audioQueueSize = 0;

			actualVideoDelay = 0;
			actualAudioDelay = 0;
			audioFrameSize = 0;
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~VideoDebug()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::Label^  audioDelayLabel;
	protected: 
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
	private: System::Windows::Forms::Label^  label9;
	private: System::Windows::Forms::Label^  label10;
	private: System::Windows::Forms::Label^  videoFramesLabel;
	private: System::Windows::Forms::Label^  audioFramesLabel;
	private: System::Windows::Forms::Label^  label7;
	private: System::Windows::Forms::Label^  label8;
	private: System::Windows::Forms::Label^  videoDroppedLabel;

	private: System::Windows::Forms::Label^  audioDroppedLabel;
	private: System::Windows::Forms::Label^  label11;
	private: System::Windows::Forms::Label^  label12;
	private: System::Windows::Forms::Label^  videoQueueSizeLabel;
	private: System::Windows::Forms::Label^  audioQueueSizeLabel;
	private: System::Windows::Forms::Label^  label13;
	private: System::Windows::Forms::Label^  label14;
	private: System::Windows::Forms::Label^  actualVideoDelayLabel;
	private: System::Windows::Forms::Label^  actualAudioDelayLabel;
	private: System::Windows::Forms::Label^  label15;
	private: System::Windows::Forms::Label^  audioFrameSizeLabel;








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
			this->label9 = (gcnew System::Windows::Forms::Label());
			this->label10 = (gcnew System::Windows::Forms::Label());
			this->videoFramesLabel = (gcnew System::Windows::Forms::Label());
			this->audioFramesLabel = (gcnew System::Windows::Forms::Label());
			this->label7 = (gcnew System::Windows::Forms::Label());
			this->label8 = (gcnew System::Windows::Forms::Label());
			this->videoDroppedLabel = (gcnew System::Windows::Forms::Label());
			this->audioDroppedLabel = (gcnew System::Windows::Forms::Label());
			this->label11 = (gcnew System::Windows::Forms::Label());
			this->label12 = (gcnew System::Windows::Forms::Label());
			this->videoQueueSizeLabel = (gcnew System::Windows::Forms::Label());
			this->audioQueueSizeLabel = (gcnew System::Windows::Forms::Label());
			this->label13 = (gcnew System::Windows::Forms::Label());
			this->label14 = (gcnew System::Windows::Forms::Label());
			this->actualVideoDelayLabel = (gcnew System::Windows::Forms::Label());
			this->actualAudioDelayLabel = (gcnew System::Windows::Forms::Label());
			this->label15 = (gcnew System::Windows::Forms::Label());
			this->audioFrameSizeLabel = (gcnew System::Windows::Forms::Label());
			this->SuspendLayout();
			// 
			// audioDelayLabel
			// 
			this->audioDelayLabel->AutoSize = true;
			this->audioDelayLabel->Location = System::Drawing::Point(115, 192);
			this->audioDelayLabel->Name = L"audioDelayLabel";
			this->audioDelayLabel->Size = System::Drawing::Size(18, 20);
			this->audioDelayLabel->TabIndex = 26;
			this->audioDelayLabel->Text = L"0";
			// 
			// videoDelayLabel
			// 
			this->videoDelayLabel->AutoSize = true;
			this->videoDelayLabel->Location = System::Drawing::Point(115, 172);
			this->videoDelayLabel->Name = L"videoDelayLabel";
			this->videoDelayLabel->Size = System::Drawing::Size(18, 20);
			this->videoDelayLabel->TabIndex = 25;
			this->videoDelayLabel->Text = L"0";
			// 
			// label5
			// 
			this->label5->AutoSize = true;
			this->label5->Location = System::Drawing::Point(12, 192);
			this->label5->Name = L"label5";
			this->label5->Size = System::Drawing::Size(98, 20);
			this->label5->TabIndex = 24;
			this->label5->Text = L"Audio Delay:";
			// 
			// label6
			// 
			this->label6->AutoSize = true;
			this->label6->Location = System::Drawing::Point(12, 172);
			this->label6->Name = L"label6";
			this->label6->Size = System::Drawing::Size(98, 20);
			this->label6->TabIndex = 23;
			this->label6->Text = L"Video Delay:";
			// 
			// audioSyncLabel
			// 
			this->audioSyncLabel->AutoSize = true;
			this->audioSyncLabel->Location = System::Drawing::Point(106, 134);
			this->audioSyncLabel->Name = L"audioSyncLabel";
			this->audioSyncLabel->Size = System::Drawing::Size(18, 20);
			this->audioSyncLabel->TabIndex = 22;
			this->audioSyncLabel->Text = L"0";
			// 
			// videoSyncLabel
			// 
			this->videoSyncLabel->AutoSize = true;
			this->videoSyncLabel->Location = System::Drawing::Point(106, 114);
			this->videoSyncLabel->Name = L"videoSyncLabel";
			this->videoSyncLabel->Size = System::Drawing::Size(18, 20);
			this->videoSyncLabel->TabIndex = 21;
			this->videoSyncLabel->Text = L"0";
			// 
			// label4
			// 
			this->label4->AutoSize = true;
			this->label4->Location = System::Drawing::Point(12, 134);
			this->label4->Name = L"label4";
			this->label4->Size = System::Drawing::Size(93, 20);
			this->label4->TabIndex = 20;
			this->label4->Text = L"Audio Sync:";
			// 
			// label3
			// 
			this->label3->AutoSize = true;
			this->label3->Location = System::Drawing::Point(12, 114);
			this->label3->Name = L"label3";
			this->label3->Size = System::Drawing::Size(93, 20);
			this->label3->TabIndex = 19;
			this->label3->Text = L"Video Sync:";
			// 
			// audioQueueLabel
			// 
			this->audioQueueLabel->AutoSize = true;
			this->audioQueueLabel->Location = System::Drawing::Point(115, 83);
			this->audioQueueLabel->Name = L"audioQueueLabel";
			this->audioQueueLabel->Size = System::Drawing::Size(31, 20);
			this->audioQueueLabel->TabIndex = 18;
			this->audioQueueLabel->Text = L"0/0";
			// 
			// videoQueueLabel
			// 
			this->videoQueueLabel->AutoSize = true;
			this->videoQueueLabel->Location = System::Drawing::Point(115, 63);
			this->videoQueueLabel->Name = L"videoQueueLabel";
			this->videoQueueLabel->Size = System::Drawing::Size(31, 20);
			this->videoQueueLabel->TabIndex = 17;
			this->videoQueueLabel->Text = L"0/0";
			// 
			// label2
			// 
			this->label2->AutoSize = true;
			this->label2->Location = System::Drawing::Point(12, 83);
			this->label2->Name = L"label2";
			this->label2->Size = System::Drawing::Size(106, 20);
			this->label2->TabIndex = 16;
			this->label2->Text = L"Audio Queue:";
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(12, 63);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(106, 20);
			this->label1->TabIndex = 15;
			this->label1->Text = L"Video Queue:";
			// 
			// label9
			// 
			this->label9->AutoSize = true;
			this->label9->Location = System::Drawing::Point(12, 9);
			this->label9->Name = L"label9";
			this->label9->Size = System::Drawing::Size(112, 20);
			this->label9->TabIndex = 27;
			this->label9->Text = L"Video Frames:";
			// 
			// label10
			// 
			this->label10->AutoSize = true;
			this->label10->Location = System::Drawing::Point(12, 29);
			this->label10->Name = L"label10";
			this->label10->Size = System::Drawing::Size(112, 20);
			this->label10->TabIndex = 28;
			this->label10->Text = L"Audio Frames:";
			// 
			// videoFramesLabel
			// 
			this->videoFramesLabel->AutoSize = true;
			this->videoFramesLabel->Location = System::Drawing::Point(128, 9);
			this->videoFramesLabel->Name = L"videoFramesLabel";
			this->videoFramesLabel->Size = System::Drawing::Size(18, 20);
			this->videoFramesLabel->TabIndex = 29;
			this->videoFramesLabel->Text = L"0";
			// 
			// audioFramesLabel
			// 
			this->audioFramesLabel->AutoSize = true;
			this->audioFramesLabel->Location = System::Drawing::Point(128, 29);
			this->audioFramesLabel->Name = L"audioFramesLabel";
			this->audioFramesLabel->Size = System::Drawing::Size(18, 20);
			this->audioFramesLabel->TabIndex = 30;
			this->audioFramesLabel->Text = L"0";
			// 
			// label7
			// 
			this->label7->AutoSize = true;
			this->label7->Location = System::Drawing::Point(264, 9);
			this->label7->Name = L"label7";
			this->label7->Size = System::Drawing::Size(75, 20);
			this->label7->TabIndex = 31;
			this->label7->Text = L"Dropped:";
			// 
			// label8
			// 
			this->label8->AutoSize = true;
			this->label8->Location = System::Drawing::Point(264, 29);
			this->label8->Name = L"label8";
			this->label8->Size = System::Drawing::Size(73, 20);
			this->label8->TabIndex = 32;
			this->label8->Text = L"Sped up:";
			// 
			// videoDroppedLabel
			// 
			this->videoDroppedLabel->AutoSize = true;
			this->videoDroppedLabel->Location = System::Drawing::Point(345, 9);
			this->videoDroppedLabel->Name = L"videoDroppedLabel";
			this->videoDroppedLabel->Size = System::Drawing::Size(18, 20);
			this->videoDroppedLabel->TabIndex = 33;
			this->videoDroppedLabel->Text = L"0";
			// 
			// audioDroppedLabel
			// 
			this->audioDroppedLabel->AutoSize = true;
			this->audioDroppedLabel->Location = System::Drawing::Point(345, 29);
			this->audioDroppedLabel->Name = L"audioDroppedLabel";
			this->audioDroppedLabel->Size = System::Drawing::Size(18, 20);
			this->audioDroppedLabel->TabIndex = 34;
			this->audioDroppedLabel->Text = L"0";
			// 
			// label11
			// 
			this->label11->AutoSize = true;
			this->label11->Location = System::Drawing::Point(264, 63);
			this->label11->Name = L"label11";
			this->label11->Size = System::Drawing::Size(44, 20);
			this->label11->TabIndex = 35;
			this->label11->Text = L"Size:";
			// 
			// label12
			// 
			this->label12->AutoSize = true;
			this->label12->Location = System::Drawing::Point(264, 83);
			this->label12->Name = L"label12";
			this->label12->Size = System::Drawing::Size(44, 20);
			this->label12->TabIndex = 36;
			this->label12->Text = L"Size:";
			// 
			// videoQueueSizeLabel
			// 
			this->videoQueueSizeLabel->AutoSize = true;
			this->videoQueueSizeLabel->Location = System::Drawing::Point(314, 63);
			this->videoQueueSizeLabel->Name = L"videoQueueSizeLabel";
			this->videoQueueSizeLabel->Size = System::Drawing::Size(18, 20);
			this->videoQueueSizeLabel->TabIndex = 37;
			this->videoQueueSizeLabel->Text = L"0";
			// 
			// audioQueueSizeLabel
			// 
			this->audioQueueSizeLabel->AutoSize = true;
			this->audioQueueSizeLabel->Location = System::Drawing::Point(314, 83);
			this->audioQueueSizeLabel->Name = L"audioQueueSizeLabel";
			this->audioQueueSizeLabel->Size = System::Drawing::Size(18, 20);
			this->audioQueueSizeLabel->TabIndex = 38;
			this->audioQueueSizeLabel->Text = L"0";
			// 
			// label13
			// 
			this->label13->AutoSize = true;
			this->label13->Location = System::Drawing::Point(197, 192);
			this->label13->Name = L"label13";
			this->label13->Size = System::Drawing::Size(102, 20);
			this->label13->TabIndex = 39;
			this->label13->Text = L"Actual Delay:";
			// 
			// label14
			// 
			this->label14->AutoSize = true;
			this->label14->Location = System::Drawing::Point(197, 172);
			this->label14->Name = L"label14";
			this->label14->Size = System::Drawing::Size(102, 20);
			this->label14->TabIndex = 40;
			this->label14->Text = L"Actual Delay:";
			// 
			// actualVideoDelayLabel
			// 
			this->actualVideoDelayLabel->AutoSize = true;
			this->actualVideoDelayLabel->Location = System::Drawing::Point(305, 172);
			this->actualVideoDelayLabel->Name = L"actualVideoDelayLabel";
			this->actualVideoDelayLabel->Size = System::Drawing::Size(18, 20);
			this->actualVideoDelayLabel->TabIndex = 41;
			this->actualVideoDelayLabel->Text = L"0";
			// 
			// actualAudioDelayLabel
			// 
			this->actualAudioDelayLabel->AutoSize = true;
			this->actualAudioDelayLabel->Location = System::Drawing::Point(305, 192);
			this->actualAudioDelayLabel->Name = L"actualAudioDelayLabel";
			this->actualAudioDelayLabel->Size = System::Drawing::Size(18, 20);
			this->actualAudioDelayLabel->TabIndex = 42;
			this->actualAudioDelayLabel->Text = L"0";
			// 
			// label15
			// 
			this->label15->AutoSize = true;
			this->label15->Location = System::Drawing::Point(417, 192);
			this->label15->Name = L"label15";
			this->label15->Size = System::Drawing::Size(94, 20);
			this->label15->TabIndex = 43;
			this->label15->Text = L"Frame Size:";
			// 
			// audioFrameSizeLabel
			// 
			this->audioFrameSizeLabel->AutoSize = true;
			this->audioFrameSizeLabel->Location = System::Drawing::Point(517, 192);
			this->audioFrameSizeLabel->Name = L"audioFrameSizeLabel";
			this->audioFrameSizeLabel->Size = System::Drawing::Size(18, 20);
			this->audioFrameSizeLabel->TabIndex = 44;
			this->audioFrameSizeLabel->Text = L"0";
			// 
			// VideoDebug
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(9, 20);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(621, 232);
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
			this->Name = L"VideoDebug";
			this->Text = L"VideoDebug";
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion

private:

	double audioDelay;
	double videoDelay;

	double audioSync;
	double videoSync;

	int audioQueue;
	int audioQueueSize;

	int videoQueue;
	int videoQueueSize;

	__int64 videoFrames;
	__int64 audioFrames;

	__int64 videoDropped;
	__int64 audioDropped;

	int videoQueueSizeBytes;
	int audioQueueSizeBytes;

	double actualVideoDelay;
	double actualAudioDelay;
	int audioFrameSize;

public:

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

	property int AudioQueue {

		int get() {

			return(audioQueue);
		}

		void set(int value) {

			audioQueue = value;
		}
	}

	property int VideoQueue {

		int get() {

			return(videoQueue);
		}

		void set(int value) {

			videoQueue = value;
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

	property __int64 VideoDropped {

		__int64 get() {

			return(videoDropped);
		}

		void set(__int64 value) {

			videoDropped = value;
		}
	}

	property __int64 AudioDropped {

		__int64 get() {

			return(audioDropped);
		}

		void set(__int64 value) {

			audioDropped = value;
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

	property int AudioFrameSize {

		int get() {

			return(audioFrameSize);
		}

		void set(int value) {

			audioFrameSize = value;
		}
	}


	};
}
