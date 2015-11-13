using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace MediaViewer.Infrastructure.Pubsubhubbub
{
    public enum PushVerifyType
    {
        Async,
        Sync
    }
    public enum PushSubscriberMode
    {
        Subscribe,
        Unsubscribe
    }
    public static class PushSubscriber
    {
        public const string DefaultSubscribeHub = "http://pubsubhubbub.appspot.com/subscribe";

        // for a permanent subscription, specify 0 for subscribeSeconds
        public static HttpStatusCode Subscribe(string hubUrl, string callback,
            string topic, PushVerifyType verify, int subscribeSeconds,
            string verifyToken, string secret)
        {
            return InternalSubscribe(hubUrl, callback, topic,
                PushSubscriberMode.Subscribe, verify, subscribeSeconds,
                verifyToken, secret);
        }

        public static HttpStatusCode Unsubscribe(string hubUrl, string callback,
            string topic, PushVerifyType verify, string verifyToken)
        {
            return InternalSubscribe(hubUrl, callback, topic,
                PushSubscriberMode.Unsubscribe, verify, -1, verifyToken, null);
        }

        private static HttpStatusCode InternalSubscribe(string hubUrl, string callback,
            string topic, PushSubscriberMode mode, PushVerifyType verify,
            int subscribeSeconds, string verifyToken, string secret)
        {
            Debug.WriteLine(string.Format("{0}: topic = {1}", mode, topic));
            StringBuilder sb = new StringBuilder();
            Action<string, string> AddParam = (key, value) =>
            {
                if (sb.Length != 0)
                {
                    sb.Append('&');
                }
                sb.AppendFormat("{0}={1}", key, HttpUtility.UrlEncode(value));
            };

            AddParam("hub.callback", callback);
            AddParam("hub.topic", topic);
            AddParam("hub.mode", mode.ToString().ToLower());
            AddParam("hub.verify", verify.ToString().ToLower());
            if (subscribeSeconds > 0)
            {
                AddParam("hub.lease_seconds", subscribeSeconds.ToString());
            }
            if (!string.IsNullOrEmpty(verifyToken))
            {
                AddParam("hub.verify_token", verifyToken);
            }
            if (!string.IsNullOrEmpty(secret))
            {
                AddParam("hub.secret", secret);
            }
            byte[] postData = Encoding.UTF8.GetBytes(sb.ToString());
            Debug.WriteLine(string.Format("  hub={0}\r\n  params = {1}", hubUrl, sb.ToString()));
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(hubUrl);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postData.Length;
            Stream requestStream = request.GetRequestStream();
            requestStream.Write(postData, 0, postData.Length);
            requestStream.Flush();
            requestStream.Close();
            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    Debug.WriteLine(string.Format("Received response ({0}).", response.StatusCode));
                    return response.StatusCode;
                }
            }
            catch (WebException wex)
            {
                Debug.WriteLine(string.Format("Exception getting response.  Status = {0}", wex.Status));
                if (wex.Response != null)
                {
                    HttpWebResponse resp = wex.Response as HttpWebResponse;
                    if (resp != null)
                    {
                        Debug.WriteLine(string.Format("Response status = {0}", resp.StatusCode));
                        return resp.StatusCode;
                    }
                }
                return HttpStatusCode.BadRequest;
            }
        }
    }
}
