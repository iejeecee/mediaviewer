using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using log4net;
using MediaViewer.Infrastructure.Logging;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml;
using VideoPlayerControl;

namespace MediaViewer.Logging
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogView : Window
    {

        LogViewModel ViewModel { get; set; }
        private char[] trimChars;

        public LogView()
        {
            InitializeComponent();

            trimChars = new char[1];
            trimChars[0] = '\n';

            //logTextBox.Document.PageWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
		                       
            Loaded += new RoutedEventHandler((o, e) =>
            {              
                logTextBox.ScrollToEnd();                
            });

            Closing += LogView_Closing;

            VisualAppender visualAppender = (VisualAppender)LogManager.GetRepository().GetAppenders().Single(t => t.Name.Equals("VisualAppender"));
            DataContext = ViewModel = visualAppender.LogViewModel;

            lock (ViewModel.MessagesLock)
            {
                foreach (LogMessageModel message in ViewModel.Messages)
                {
                    addMessage(message);
                }
            }

            ViewModel.Messages.CollectionChanged += messagesCollectionChangedCallback;
            ViewModel.LogLevel.CurrentChanged += LogLevel_CurrentChanged;
     
            using (Stream s = Assembly.GetExecutingAssembly().GetManifestResourceStream("MediaViewer.Logging.LogSyntaxHighlighting.xshd"))
            {
                using (XmlTextReader reader = new XmlTextReader(s))
                {
                    logTextBox.SyntaxHighlighting = HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }

        void LogLevel_CurrentChanged(object sender, EventArgs e)
        {
            VideoPlayerViewModel.enableLibAVLogging((LogMessageModel.LogLevel)ViewModel.LogLevel.CurrentItem);
        }

        void LogView_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ViewModel.Messages.CollectionChanged -= messagesCollectionChangedCallback;
        }
        
        void messagesCollectionChangedCallback(Object sender, NotifyCollectionChangedEventArgs e)
        {
            Application.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    logTextBox.Document.Text = null;
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach (LogMessageModel message in e.NewItems)
                    {
                        addMessage(message);
                    }

                }

            }));

        }

          
        private void addMessage(LogMessageModel message)
        {
            if ((int)message.Level > (int)ViewModel.LogLevel.CurrentItem)
            {
                return;
            }

            String text = message.Text.TrimEnd(trimChars);
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\n", "\r\t");

            if (logTextBox.Document.LineCount == LogViewModel.maxLinesInLog)
            {
                int length = logTextBox.Document.GetOffset(2, 0);
                logTextBox.Document.Remove(0, length);
            
            }

            int offset = logTextBox.Document.GetOffset(logTextBox.Document.LineCount, 0);
            logTextBox.Document.Insert(offset, text);
            
            logTextBox.ScrollToEnd();
            
        }
            
      
    }
}
