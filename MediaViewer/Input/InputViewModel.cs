using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Input
{
    class InputViewModel : CloseableBindableBase
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
            set { 
            SetProperty(ref title, value);
            }
        }

        String inputText;

        public String InputText
        {
            get { return inputText; }
            set { 
            SetProperty(ref inputText, value);
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
