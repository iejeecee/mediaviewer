using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MediaViewer.Logging
{
    class LogViewModel : BindableBase
    {
        const int maxLinesInLog = 100;

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

            Application.Current.Dispatcher.BeginInvoke(new Action(() => {

                if (Messages.Count > maxLinesInLog)                
                {
                    Messages.RemoveAt(0);
                }
                
                Messages.Add(message);

            }));

        }


        Command clearLog;

        public Command ClearLog
        {
            get { return clearLog; }
            set { clearLog = value; }
        }
    }
}
