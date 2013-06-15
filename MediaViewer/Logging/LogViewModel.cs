using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.Logging
{
    class LogViewModel : ObservableObject
    {
        public LogViewModel()
        {
            messages = new ObservableCollection<LogMessageModel>();

            clearLog = new Command(new Action(() => messages.Clear()));
        }

        ObservableCollection<LogMessageModel> messages;

        public ObservableCollection<LogMessageModel> Messages
        {
            get { return messages; }
            set { messages = value; }
        }

        public void addMessage(LogMessageModel message) {

            if (Application.Current.Dispatcher.CheckAccess())
            {
                Messages.Add(message);
            }
            else
            {
                Application.Current.Dispatcher.BeginInvoke(
                    new Action(() => Messages.Add(message)));
            }
        }

        Command clearLog;

        public Command ClearLog
        {
            get { return clearLog; }
            set { clearLog = value; }
        }
    }
}
