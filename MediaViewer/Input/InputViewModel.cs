using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Input
{
    class InputViewModel : CloseableObservableObject
    {

        public InputViewModel()
        {
            InputText = "";
            InputHistory = new ObservableCollection<string>();
            Title = "";

            OkCommand = new Command(new Action(() =>
            {
                OnClosingRequest(new DialogEventArgs(DialogMode.SUBMIT));
            }));

            CancelCommand = new Command(new Action(() =>
              {
                  OnClosingRequest(new DialogEventArgs(DialogMode.CANCEL));
              }));

        }

        String title;

        public String Title
        {
            get { return title; }
            set { title = value;
            NotifyPropertyChanged();
            }
        }

        String inputText;

        public String InputText
        {
            get { return inputText; }
            set { inputText = value;
            NotifyPropertyChanged();
            }
        }

        ObservableCollection<String> inputHistory;

        public ObservableCollection<String> InputHistory
        {
            get { return inputHistory; }
            set { inputHistory = value; }
        }

        Command okCommand;

        public Command OkCommand
        {
            get { return okCommand; }
            set { okCommand = value; }
        }
        Command cancelCommand;

        public Command CancelCommand
        {
            get { return cancelCommand; }
            set { cancelCommand = value; }
        }
        
    }
}
