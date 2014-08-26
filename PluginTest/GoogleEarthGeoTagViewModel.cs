using HtmlAgilityPack;
using MediaViewer.MediaFileBrowser;
using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Resources;

namespace PluginTest
{
    [Export]
    public class GoogleEarthGeoTagViewModel : ObservableObject, IDisposable
    {
       
        GeoTagScriptInterface script;
      
        [ImportingConstructor]
        public GoogleEarthGeoTagViewModel(MediaFileBrowserView mediaFileBrowserView)
        {
            WebBrowser = new System.Windows.Controls.WebBrowser();
       
            // get html and javascript embedded resources
            Stream htmlStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PluginTest.GeoTag.html");
            Stream scriptStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("PluginTest.GeoTag.js");

            // insert javascript into html document
            scriptStream.Position = 0;
            String scriptString;
            using (StreamReader reader = new StreamReader(scriptStream, Encoding.UTF8))
            {
                scriptString = reader.ReadToEnd();
            }

            scriptStream.Close();

            HtmlDocument document = new HtmlDocument();
            document.Load(htmlStream);

            htmlStream.Close();

            HtmlNode head = document.DocumentNode.SelectSingleNode("/html/head");
            HtmlNode node = document.CreateElement("script");
            head.AppendChild(node);
            node.SetAttributeValue("type", "text/javascript");
            node.AppendChild(document.CreateTextNode(scriptString));
       
            // navigate to html document in webbrowser
            MemoryStream stream = new MemoryStream();
            //document.Save("d:\\mijnhtml.html");
            document.Save(stream);
            stream.Position = 0;                               

            WebBrowser.NavigateToStream(stream);

            List<GeoTagFileData> geoTagFileItems = new List<GeoTagFileData>();
/*
            foreach (MediaFileItem item in items)
            {
                GeoTagFileData geoTagFileItem = new GeoTagFileData(item);
                geoTagFileItems.Add(geoTagFileItem);
            }
*/
            script = new GeoTagScriptInterface(WebBrowser, geoTagFileItems);

            WebBrowser.ObjectForScripting = script;

            script.Initialized += script_Initialized;

            searchCommand = new Command(() =>
            {
                if (String.IsNullOrEmpty(searchText) || String.IsNullOrWhiteSpace(searchText)) return;

                script.flyTo(searchText);
            });
        }

        void script_Initialized(object sender, EventArgs e)
        {
            if (Loaded != null)
            {
                Loaded(this, EventArgs.Empty);
            }
        }

        WebBrowser webBrowser;

        public WebBrowser WebBrowser
        {
            get { return webBrowser; }
            set { webBrowser = value;
            NotifyPropertyChanged();
            }
        }

     
        public void Dispose()
        {
            if (WebBrowser != null)
            {
                WebBrowser.Dispose();
                WebBrowser = null;
            }
        }

        public event EventHandler Loaded;

        String searchText;

        public String SearchText
        {
            get { return searchText; }
            set { searchText = value;
            NotifyPropertyChanged();
            }
        }

        Command searchCommand;

        public Command SearchCommand {

            set {

            }
            get {
                return (searchCommand);
            }
        }
        
        public MediaFileItem SelectedMedia
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

   
    }
}
