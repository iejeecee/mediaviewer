using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text;
using System.Xml;

namespace MediaViewer.Infrastructure.Pubsubhubbub
{
    public class PushVerifyEventArgs : EventArgs
    {
        public PushSubscriberMode Mode { get; private set; }
        public string Topic { get; private set; }
        public string Challenge { get; private set; }
        public int LeaseSeconds { get; private set; }
        public string VerifyToken { get; private set; }
        public bool Allow { get; set; }
        public string Response { get; set; }
        public PushVerifyEventArgs(PushSubscriberMode hubMode, string hubTopic,
            string hubChallenge, int hubLeaseSeconds, string hubVerifyToken)
        {
            Allow = false;
            Mode = hubMode;
            Topic = hubTopic;
            Challenge = hubChallenge;
            LeaseSeconds = hubLeaseSeconds;
            VerifyToken = hubVerifyToken;
        }
    }

    public delegate void PushVerifyDelegate(object sender, PushVerifyEventArgs args);
    public class PushPostEventArgs : EventArgs
    {
        public string ContentType { get; private set; }
        public string HubSignature { get; private set; }
        public SyndicationFeed Feed { get; private set; }
        public PushPostEventArgs(string ctype, string sig, SyndicationFeed feed)
        {
            ContentType = ctype;
            HubSignature = sig;
            Feed = feed;
        }
    }

    public delegate void PushPostDelegate(object sender, PushPostEventArgs args);

    // This is a subscriber callback for Pubsubhubbub.
    // This is the Web service that is called by the hub:
    // 1. To verify a subscription.
    // 2. To post updates when subscribed feeds update.
    //
    // This class is a very lightweight HTTP client that uses the HttpListener class.
    //
    // If you want to build a server that uses the HttpListener, then you have to
    // either run that program as administrator, or from an administrator command prompt,
    // register the url.  Like this:
    //  netsh http add urlacl http://+:8080/ user=Everyone listen=true
    // If that succeeds, then you can run the program without having to be in Administrator mode.
    // The Prefix that you add to the HttpListener should be "http://+:8080/"
    public class PushSubscriberCallback : IDisposable
    {
        protected HttpListener Listener;
        public object UserData { get; set; }
        // Events!
        public event PushVerifyDelegate PushVerify;
        public event PushPostDelegate PushPost;

        public PushSubscriberCallback(string prefix)
        {
            Listener = new HttpListener();
            Listener.Prefixes.Add(prefix);
        }

        ~PushSubscriberCallback()
        {
            this.Dispose(false);
        }

        public virtual void Start()
        {
            Debug.WriteLine("Start");
            Listener.Start();
            ListenForConnection();
        }

        protected virtual void ListenForConnection()
        {
            Listener.BeginGetContext(ListenerCallback, UserData);
            Debug.WriteLine("Listening for connection.");
        }

        public virtual void Stop()
        {
            Debug.WriteLine("Stop");
            Listener.Stop();
        }

        protected virtual void ListenerCallback(IAsyncResult ar)
        {
            try
            {
                Debug.WriteLine(string.Format("\r\n{0}: Connection made", DateTime.Now));
                Debug.Indent();
                HttpListenerContext context;
                try
                {
                    context = Listener.EndGetContext(ar);
                }
                catch (ObjectDisposedException)
                {
                    // This method could be called after the listener has been disposed.
                    // This catches that exception and just returns.
                    return;
                }
                // Listen for next connection
                ListenForConnection();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                Debug.WriteLine(string.Format("Request from {0}", request.RemoteEndPoint.Address.ToString()));
                Debug.WriteLine(string.Format("Method: {0}", request.HttpMethod));
                Debug.WriteLine(string.Format("RawUrl: {0}", request.RawUrl));
                Debug.WriteLine(string.Format("Headers:"));
                foreach (var key in request.Headers.AllKeys)
                {
                    Debug.WriteLine(string.Format("  {0}={1}", key, request.Headers[key]));
                }
                response.AddHeader("Content-Type", "text/plain");
                switch (request.HttpMethod)
                {
                    case "GET":
                        {
                            // A GET is a subscribe or unsubscribe request.
                            string responseMessage = string.Empty;
                            int rslt = VerifySubscribeRequest(request, out responseMessage);
                            Debug.WriteLine(string.Format("response = {0}", responseMessage));
                            response.StatusCode = rslt;
                            byte[] responseText = Encoding.UTF8.GetBytes(responseMessage);// + "\r\n");
                            response.ContentLength64 = responseText.Length;
                            response.OutputStream.Write(responseText, 0, responseText.Length);
                            break;
                        }
                    case "POST":
                        {
                            // A POST is content delivery.
                            DoPost(request, response.Headers);
                            response.StatusCode = (int)HttpStatusCode.OK;
                            break;
                        }
                    default:
                        response.StatusCode = (int)HttpStatusCode.BadRequest;
                        break;
                }
                // Must close output stream in order to send data!
                response.OutputStream.Close();
                Debug.WriteLine(string.Format("Returned status code {0}", response.StatusCode));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("Exception in callback handler.\r\n{0}", ex));
            }
            Debug.Unindent();
        }

        protected virtual void DoPost(HttpListenerRequest request, NameValueCollection headers)
        {
            string contentType = request.ContentType;
            string sig = request.Headers["X-Hub-Signature"];
            string hubSignature = null;
            if (!string.IsNullOrEmpty(sig) && sig.Length > 4)
            {
                hubSignature = sig.Substring(4);
            }
            SyndicationFeed feed = null;
            try
            {
                using (var reader = new XmlTextReader(request.InputStream))
                {
                    feed = SyndicationFeed.Load(reader);
                }
            }
            catch
            {
                // There was something wrong with the feed.
                // Just ignore it and return.
                return;
            }
            var args = new PushPostEventArgs(contentType, hubSignature, feed);
            OnPost(args);
        }

        protected virtual void OnPost(PushPostEventArgs args)
        {
            if (PushPost != null)
            {
                PushPost(this, args);
            }
        }

        protected virtual int VerifySubscribeRequest(HttpListenerRequest request, out string responseMessage)
        {
            var qs = request.QueryString;
            PushSubscriberMode mode;
            int leaseSeconds;
            string msg = null;
            Func<string, bool> verifyParam = (paramName) =>
            {
                if (string.IsNullOrEmpty(qs[paramName]))
                {
                    msg = "Expected " + paramName;
                    return false;
                }
                return true;
            };
            if (!(verifyParam("hub.mode") && verifyParam("hub.topic") && verifyParam("hub.challenge")))
            {
                responseMessage = msg;
                return (int)HttpStatusCode.BadRequest;
            }
            try
            {
                mode = (PushSubscriberMode)Enum.Parse(typeof(PushSubscriberMode), qs["hub.mode"], true);
            }
            catch (Exception)
            {
                // Not a valid mode.
                responseMessage = "Unknown mode: " + qs["hub.mode"] ?? "";
                return (int)HttpStatusCode.BadRequest;
            }

            if (!int.TryParse(qs["hub.lease_seconds"], out leaseSeconds))
            {
                leaseSeconds = 0;
                if (mode == PushSubscriberMode.Subscribe)
                {
                    responseMessage = "Expected hub.lease_seconds";
                    return (int)HttpStatusCode.BadRequest;
                }
            }

            var args = new PushVerifyEventArgs(mode, qs["hub.topic"],
                qs["hub.challenge"], leaseSeconds, qs["hub.verify_token"]);
            args.Response = args.Challenge;
            args.Allow = true;
            OnVerify(args);
            responseMessage = args.Response;
            return args.Allow ? (int)HttpStatusCode.OK : (int)HttpStatusCode.NotFound;
        }

        protected virtual void OnVerify(PushVerifyEventArgs args)
        {
            if (PushVerify != null)
            {
                PushVerify(this, args);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool disposed = false;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (Listener != null)
                    {
                        Listener.Close();
                    }
                }
                disposed = true;
            }
        }
    }
}