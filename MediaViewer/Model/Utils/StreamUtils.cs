using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaViewer.Model.Utils
{
    public class StreamUtils
    {
        // 60 seconds
        const int HTTP_TIMEOUT_MS = 60 * 1000;
        const int HTTP_READ_BUFFER_SIZE_BYTES = 8096;

        public static void readHttpRequest(Uri location, Stream data, out String mimeType, CancellationToken token, Action<long,long> progressCallback = null) {

            HttpWebResponse response = null;
            Stream responseStream = null;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(location);
                request.Method = "GET";
                request.Timeout = HTTP_TIMEOUT_MS;

                IAsyncResult requestResult = request.BeginGetResponse(null, null);

                while (!requestResult.IsCompleted)
                {
                    if (token.IsCancellationRequested)
                    {
                        request.Abort();
                        throw new OperationCanceledException(token);
                    }

                    Thread.Sleep(100);
                }

                response = (HttpWebResponse)request.EndGetResponse(requestResult);
                mimeType = response.ContentType;

                responseStream = response.GetResponseStream();
                responseStream.ReadTimeout = HTTP_TIMEOUT_MS;
               
                int bufSize = HTTP_READ_BUFFER_SIZE_BYTES;
                int count = 0;

                byte[] buffer = new byte[bufSize];
                long total = 0;

                while ((count = responseStream.Read(buffer, 0, bufSize)) > 0)
                {
                    if (token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException(token);
                    }

                    data.Write(buffer, 0, count);

                    if (progressCallback != null)
                    {
                        total += count;
                        progressCallback(total, response.ContentLength);
                    }
                }

                data.Seek(0, System.IO.SeekOrigin.Begin);
            }
            finally
            {
                if (responseStream != null)
                {
                    responseStream.Close();
                }

                if (response != null)
                {
                    response.Close();
                }
            }
        }
    }
}
