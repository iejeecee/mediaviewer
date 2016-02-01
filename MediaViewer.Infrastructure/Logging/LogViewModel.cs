using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace MediaViewer.Infrastructure.Logging
{
    public class LogViewModel : BindableBase
    {
        public Object MessagesLock { get; protected set; }
        public const int maxLinesInLog = 1000;

        public LogViewModel()
        {
            MessagesLock = new Object();
            messages = new ObservableCollection<LogMessageModel>();

            BindingOperations.EnableCollectionSynchronization(messages, MessagesLock);
            //clearLog = new Command(new Action(() => messages.Clear()));

            LogLevel = new CollectionView(Enum.GetValues(typeof(LogMessageModel.LogLevel)));
            LogLevel.MoveCurrentTo(LogMessageModel.LogLevel.INFO);
        }

        public CollectionView LogLevel { get; set; }

        ObservableCollection<LogMessageModel> messages;

        public ObservableCollection<LogMessageModel> Messages
        {
            get { return messages; }
            set { messages = value; }
        }

        public void addMessage(LogMessageModel message) {

            lock (MessagesLock)
            {
                if (Messages.Count > maxLinesInLog)
                {
                    Messages.RemoveAt(0);
                }

                Messages.Add(message);
            }

        }


        /*Command clearLog;

        public Command ClearLog
        {
            get { return clearLog; }
            set { clearLog = value; }
        }*/
    }
}
