using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvvmFoundation.Wpf;
using System.Windows;
using MediaViewer.Utils;

namespace MediaViewer
{
    class MainWindowViewModel : ObservableObject
    {
        protected static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string currentImageLocation;

        public string CurrentImageLocation
        {
            get { return currentImageLocation; }
            set
            {
                currentImageLocation = value;
                NotifyPropertyChanged();
            }
        }
        string currentVideoLocation;

        public string CurrentVideoLocation
        {
            get { return currentVideoLocation; }
            set
            {
                currentVideoLocation = value;
                NotifyPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {

            // recieve messages requesting the display of media items
          
            GlobalMessenger.Instance.Register<string>("MainWindowViewModel.ViewMediaCommand", new Action<string>((fileName) =>
            {
                ViewMediaCommand.DoExecute(fileName);
            }));

            ViewMediaCommand = new Command(new Action<object>((location) =>
              {
                  String mimeType = MediaFormatConvert.fileNameToMimeType((string)location);

                  if (mimeType.StartsWith("image"))
                  {
                      CurrentImageLocation = (string)location;
                  }
                  else if (mimeType.StartsWith("video"))
                  {
                      CurrentVideoLocation = (string)location;
                  }
                  else
                  {
                      log.Warn("Trying to view media of unknown mime type: " + (string)location + ", mime type: " + mimeType);
                  }

              }));
        }

        Command viewMediaCommand;

        public Command ViewMediaCommand
        {
            get { return viewMediaCommand; }
            set { viewMediaCommand = value; }
        }

    }
}
