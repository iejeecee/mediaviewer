#pragma once
#include "ProgressForm.h"
#include "FileUtils.h"

#using  <System.Web.dll>

using namespace System::Web;

namespace imageviewer {

	public ref class DownloadArgs {

	public:

		List<String ^> ^url;
		String ^outputDir;

		DownloadArgs(List<String ^> ^url, String ^outputDir) {

			this->url = url;
			this->outputDir = outputDir;
		}

	};

	public ref class DownloadProgressForm : public ProgressForm {

	private:

		static log4net::ILog ^log = log4net::LogManager::GetLogger(System::Reflection::MethodBase::GetCurrentMethod()->DeclaringType);

		String ^decodeUrl(String ^url) {

			while(url->Contains(L"%")) {

				int length = url->Length;

				url = System::Web::HttpUtility::UrlDecode(url);

				if(length == url->Length) break;

			}

			return(url);
		}

	protected:

		virtual void asyncAction(Object ^state) override {

			DownloadArgs ^args = dynamic_cast<DownloadArgs ^>(state);

			List<String ^> ^url = args->url;
			String ^outputDir = args->outputDir;
			String ^name = L"";

			for(int i = 0; i < url->Count; i++) {

				try {

					totalProgressValue = i;
					itemInfo = name;
					itemProgressValue = 0;

					HttpWebRequest ^request = (HttpWebRequest^)WebRequest::Create(url[i]);
					request->Method = L"GET";
					request->Timeout = 60 * 1000;

					IAsyncResult ^requestResult = request->BeginGetResponse(nullptr, nullptr);

					while(!requestResult->IsCompleted) {

						if(abortAsyncAction == true) {

							request->Abort();
							return;
						}

						Thread::Sleep(100);

					}

					HttpWebResponse ^response = dynamic_cast<HttpWebResponse ^>(request->EndGetResponse(requestResult));
					Stream ^onlineData = response->GetResponseStream();
					onlineData->ReadTimeout = 60 * 1000;

					name = System::IO::Path::GetFileNameWithoutExtension(request->RequestUri->AbsoluteUri);

					if(!String::IsNullOrEmpty(name)) {

						name = decodeUrl(name);
						name = Util::removeIllegalCharsFromFileName(name);
					} 

					if(String::IsNullOrEmpty(name) || name->Length > 200) {

						name = System::IO::Path::GetRandomFileName();
					}

					name = name + L"." + MediaFormatConvert::mimeTypeToExtension(response->ContentType);

					String ^outputFilename = FileUtils::getUniqueFileName(outputDir + "\\" + name);

					Stream ^outStream = File::OpenWrite(outputFilename);

					int bufSize = 8096;
					int count = 0;

					int bytesRead = 0;
					int updateSize = 10240;
					int nextUpdateSize = updateSize;

					cli::array<unsigned char> ^buffer = gcnew cli::array<unsigned char>(bufSize);					

					while((count = onlineData->Read(buffer, 0, bufSize)) > 0) {

						if(abortAsyncAction == true) {							

							response->Close();
							return;
						}

						outStream->Write(buffer, 0, count);

						if((bytesRead += count) > nextUpdateSize) {

							int fileProgress = Math::Max(0, int(100.0 / response->ContentLength * bytesRead));

							totalProgressValue = i;
							itemInfo = name;
							itemProgressValue = fileProgress;

							nextUpdateSize += updateSize;
						}

					}

					response->Close();
					outStream->Close();

				} catch(Exception ^e) {

					log->Error("Error downloading: " + url[i], e);

					String ^error = "Error downloading: " + url[i] + L"\r\n" + e->Message;			
					addInfoString(error);
				}

			}

			totalProgressValue = url->Count;
			itemInfo = name;
			itemProgressValue = 100;

			actionFinished();
		}

	public:

		DownloadProgressForm() {

		}

		void download(List<String ^> ^url, String ^outputDir) {

			totalProgressMaximum = url->Count;

			WaitCallback ^asyncDownload = gcnew WaitCallback(this, &DownloadProgressForm::asyncAction);

			DownloadArgs ^args = gcnew DownloadArgs(url, outputDir);

			ThreadPool::QueueUserWorkItem(asyncDownload, args);

		}


	};
}