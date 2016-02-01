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
		AVIOContext *avioCtx;

	public:

		enum Mode
		{
			READABLE,
			WRITEABLE
		};

		MemoryStreamAVIOContext(MemoryStream *inputStream, Mode mode)		 		  
		{
			this->inputStream = inputStream;
			
			int bufferSize = 4 * 1024;
			unsigned char *buffer = (unsigned char *)av_malloc(bufferSize);

			avioCtx = avio_alloc_context(buffer, bufferSize, mode == WRITEABLE ? 1 : 0, this, 
				&readPacket, &writePacket, &seek); 
		}

		~MemoryStreamAVIOContext() 
		{ 		
			// free buffer owned by avioCtx 	
			av_free(avioCtx->buffer);							
			av_free(avioCtx);

			avioCtx = NULL;
									
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