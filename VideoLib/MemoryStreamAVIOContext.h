#pragma once
#include "Video.h"
#include "MemoryStream.h"
/*
 my_iocontext_private priv_ctx(inputStream);
   AVFormatContext * ctx = ::avformat_alloc_context();
   ctx->pb = priv_ctx.get_avio();

   int err = avformat_open_input(&ctx, "arbitrarytext", NULL, NULL);
*/

namespace VideoLib {

	class MemoryStreamAVIOContext
	{
	private:
		
		MemoryStream *inputStream; // abstract stream interface, You can adapt it to TMemoryStream  
		int bufferSize;
		unsigned char *buffer;  
		AVIOContext *avioCtx;

	public:

		MemoryStreamAVIOContext(MemoryStream *inputStream, bool isWriteable = false)		 		  
		{
			this->inputStream = inputStream;
			bufferSize = 4 * 1024;
			buffer = (unsigned char *)av_malloc(bufferSize);

			avioCtx = avio_alloc_context(buffer, bufferSize, isWriteable ? 1 : 0, this, 
				&readPacket, &writePacket, &seek); 
		}

		~MemoryStreamAVIOContext() 
		{ 
			av_free(avioCtx);
			av_free(buffer); 
		}

		static int readPacket(void *opaque, uint8_t *buf, int bufSize) 
		{
			MemoryStreamAVIOContext *ctx = static_cast<MemoryStreamAVIOContext *>(opaque);
			return ctx->inputStream->read(buf, bufSize); 
		}

		static int writePacket(void *opaque, uint8_t *buf, int bufSize) 
		{
			MemoryStreamAVIOContext *ctx = static_cast<MemoryStreamAVIOContext *>(opaque);		
			return ctx->inputStream->write(buf, bufSize);  
		}

		static int64_t seek(void *opaque, int64_t offset, int whence) 
		{
			MemoryStreamAVIOContext *ctx = static_cast<MemoryStreamAVIOContext *>(opaque);		
			return ctx->inputStream->seek(offset, whence); 
		}

		AVIOContext *getAVIOContext() 
		{ 
			return avioCtx; 
		}

	};


}