#pragma once
using namespace System;
using namespace System::Collections::Generic;
using namespace System::IO;
using namespace System::Windows::Media::Imaging;
using namespace System::Threading;

namespace VideoLib {

	public ref class OpenVideoArgs {

		String ^videoLocation;
		String ^videoType;
		String ^audioLocation;
		String ^audioType;

	public: 

		OpenVideoArgs(String ^videoLocation) {

			VideoLocation = videoLocation;
			VideoType = nullptr;
			AudioLocation = nullptr;
			AudioType = nullptr;
		}

		OpenVideoArgs(String ^videoLocation, String ^videoType) {

			VideoLocation = videoLocation;
			VideoType = videoType;
			AudioLocation = nullptr;
			AudioType = nullptr;
		}

		OpenVideoArgs(String ^videoLocation, String ^videoType, String ^audioLocation, String ^audioType) {

			VideoLocation = videoLocation;
			VideoType = videoType;
			AudioLocation = audioLocation;
			AudioType = audioType;
		}

		property String ^VideoLocation {

			String ^get() {
				return videoLocation;
			}

			void set(String ^value) {

				videoLocation = value;
			}
		}

		property String ^VideoType {

			String ^get() {
				return videoType;
			}

			void set(String ^value) {

				videoType = value;
			}
		}

		property String ^AudioLocation {

			String ^get() {
				return audioLocation;
			}

			void set(String ^value) {

				audioLocation = value;
			}
		}

		property String ^AudioType {

			String ^get() {
				return audioType;
			}

			void set(String ^value) {

				audioType = value;
			}
		}


	};

}