#pragma once
#include <vector>
#include <fstream>
#include "..\Video\Video.h"

namespace VideoLib2 {

	class MemoryStream {

	private:

		std::vector<char> buffer;
		int64_t pos;

	public:

		MemoryStream() {

			pos = 0;
		}

		int write(unsigned char *buf, int bufSize) {

			for(int i = 0; i < bufSize; i++) {

				if(pos < size()) {

					buffer[pos] = buf[i];

				} else {

					buffer.push_back(buf[i]);
				}

				pos++;
			}
			
			return bufSize;
		}

		int read(unsigned char *buf, int bufSize) {

			if(pos + bufSize > size()) {

				bufSize = size() - pos;
			}

			if(bufSize == 0) {

				return(0);
			}

			memcpy(buf, &buffer[pos], bufSize);
			pos += bufSize;

			return bufSize;
		}

		int64_t seek(int64_t offset, int whence) {

			if(whence == AVSEEK_SIZE) {

				return(size());
			}

			unsigned int newPos;

			if(whence == SEEK_SET) {

				newPos = offset;
			} 
			else if(whence == SEEK_CUR) 
			{
				newPos = pos + offset;
			
			} 
			else if(whence == SEEK_END)
			{
				newPos = size() + offset;
			}

			if(newPos < 0 || newPos >= size()) {

				return -1;

			} else {

				pos = newPos;
				return 0;
			}

		}

		int64_t size() {

			return(buffer.size());
		}

		void saveToDisk(const char *filename)
		{
			std::ofstream output(filename, std::ios::binary);
			
			output.write(&buffer[0], buffer.size());
			output.close();

		}

	};


}