using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace PicasaLib
{
    class HttpRequest
    {
        public static String responseToString(HttpWebResponse response, int timeOutSeconds)
        {

            Stream responseStream = response.GetResponseStream();
            responseStream.ReadTimeout = timeOutSeconds * 1000;

            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            // Pipes the stream to a higher level stream reader with the required encoding format.
            StreamReader readStream = new StreamReader(responseStream, encode);

            String responseString = readStream.ReadToEnd();

            return (responseString);
        }

        public static HttpWebResponse getRequest(String requestUriString, int timeOutSeconds) {

		    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);

		    request.Method = "GET";

		    int timeOutMiliSeconds = timeOutSeconds * 1000;

		    request.Timeout = timeOutMiliSeconds;
    	
		    HttpWebResponse response = (HttpWebResponse )request.GetResponse();

		    return(response);
	    }

        public static IAsyncResult asyncGetRequest(String requestUriString, AsyncCallback callback, WaitOrTimerCallback timeoutCallback, int timeOutSeconds) {

		    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);

		    request.Method = "GET";

		    IAsyncResult result = request.BeginGetResponse(callback, request);

		    int timeOutMiliSeconds = timeOutSeconds * 1000;

		    ThreadPool.RegisterWaitForSingleObject(result.AsyncWaitHandle, timeoutCallback, request, timeOutMiliSeconds, true );

		    return(result);
	    }

        public static HttpWebResponse postRequest(String requestUriString, String content, int timeOutSeconds) {

		    bool expect100Continue = System.Net.ServicePointManager.Expect100Continue;
		    System.Net.ServicePointManager.Expect100Continue = false;

		    byte[] data = Encoding.UTF8.GetBytes(content);

		    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUriString);

		    request.Method = "POST";
		    request.ContentType = "application/x-www-form-urlencoded";
		    request.ContentLength = data.Length;
		    request.Timeout = timeOutSeconds * 1000;

		    Stream requestStream = request.GetRequestStream();
		    requestStream.WriteTimeout = timeOutSeconds * 1000;

		    requestStream.Write(data, 0, data.Length);

		    HttpWebResponse response = (HttpWebResponse )request.GetResponse();

		    System.Net.ServicePointManager.Expect100Continue = expect100Continue;

		    return(response);
	    }


    }
}
