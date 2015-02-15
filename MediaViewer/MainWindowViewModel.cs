using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.Mvvm;
using System.Windows;
using MediaViewer.MetaData;
using MediaViewer.About;
using MediaViewer.Logging;
using MediaViewer.ImagePanel;
using MediaViewer.Import;
using MediaViewer.Model.Media.File.Watcher;
using System.ComponentModel.Composition;
using MediaViewer.Model.Settings;
using MediaViewer.Model.Utils;
using MediaViewer.TagEditor;
using Microsoft.Practices.Prism.Commands;
using MediaViewer.Model.Mvvm;
using MediaViewer.Infrastructure.Logging;

namespace MediaViewer
{
    class MainWindowViewModel : BindableBase
    {
           
        protected 

        AppSettings Settings
        {
            get;
            set;
        }

        string currentImageLocation;

        public string CurrentImageLocation
        {
            get { return currentImageLocation; }
            set
            {               
                SetProperty(ref currentImageLocation, value);
            }
        }
        string currentVideoLocation;

        public string CurrentVideoLocation
        {
            get { return currentVideoLocation; }
            set
            {              
                SetProperty(ref currentVideoLocation, value);
            }
        }

        string windowTitle;

        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {         
                SetProperty(ref windowTitle, value);
            }
        }

        public MainWindowViewModel(AppSettings settings)
        {
            Settings = settings;

            WindowTitle = "MediaViewer";

            // recieve messages requesting the display of media items
/*
            GlobalMessenger.Instance.Register<string>("MainWindowViewModel.ViewMediaCommand", new Action<string>((fileName) =>
            {
                ViewMediaCommand.DoExecute(fileName);
            }));
*/
            ViewMediaCommand = new Command<String>(new Action<String>((location) =>
              {
                  if (String.IsNullOrEmpty(location)) return;

                  String mimeType = MediaFormatConvert.fileNameToMimeType(location);

                  if (mimeType.StartsWith("image"))
                  {
                      CurrentImageLocation = location;
                  }
                  else if (mimeType.StartsWith("video"))
                  {
                      CurrentVideoLocation = location;
                  }
                  else
                  {
                      Logger.Log.Warn("Trying to view media of unknown mime type: " + (string)location + ", mime type: " + mimeType);
                  }

              }));

            TagEditorCommand = new Command(() =>
                {
                    //TagEditorView tagEditor = new TagEditorView();
                    //tagEditor.ShowDialog();
                });
         

            ShowLogCommand = new Command(() =>
                {
                    //log4net.Appender.IAppender[] appenders = log4net.LogManager.GetRepository().GetAppenders();
                    //VisualAppender appender = (VisualAppender)(appenders[0]);

                    LogView logView = new LogView();
                    //logView.DataContext = appender.LogViewModel;

                    logView.Show();

                });

            ClearHistoryCommand = new Command(() =>
                {

                    if (MessageBox.Show("Clear all history?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Settings.clearHistory();
                    }

                });

            
        }

        Command<String> viewMediaCommand;

        public Command<String> ViewMediaCommand
        {
            get { return viewMediaCommand; }
            set { viewMediaCommand = value; }
        }

        Command tagEditorCommand;

        public Command TagEditorCommand
        {
            get { return tagEditorCommand; }
            set { tagEditorCommand = value; }
        }  

        Command showLogCommand;

        public Command ShowLogCommand
        {
            get { return showLogCommand; }
            set { showLogCommand = value; }
        }

        Command clearHistoryCommand;

        public Command ClearHistoryCommand
        {
            get { return clearHistoryCommand; }
            set { clearHistoryCommand = value; }
        }       

    }
}
