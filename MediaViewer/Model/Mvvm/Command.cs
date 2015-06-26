using Microsoft.Practices.Prism.Commands;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MediaViewer.Model.Mvvm
{
    public class Command : Command<object>
    {
        public Command(Action method, bool isExecutable = true)
            : base(new Action<Object>(o => method()), isExecutable)
        {

        }

        public void Execute()
        {
            Execute(null);
        }
    }

    public class Command<T> : ICommand, INotifyPropertyChanged
    {
        Action<T> Method { get; set; }
        
        public Command(Action<T> method, bool isExecutable = true)            
        {
            if (method == null)
            {
                throw new ArgumentNullException("method", "Delegate method cannot be null");
            }

            Method = method;
            IsExecutable = isExecutable;           
        }
             
        bool isExecutable;

        public bool IsExecutable
        {
            get { return isExecutable; }
            set
            {
                if (value == isExecutable) return;

                isExecutable = value;
                OnPropertyChanged("IsExecutable");
                OnCanExecuteChanged();
            }
        }

        protected void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }

        }

        protected void OnPropertyChanged(String name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        protected void OnExecuting()
        {
            if (Executing != null)
            {
                Executing(this, EventArgs.Empty);
            }
        }

        protected void OnExecuted()
        {
            if (Executed != null)
            {
                Executed(this, EventArgs.Empty);
            }
        }

        public bool CanExecute(object parameter)
        {
            return (IsExecutable);
        }
       
        public void Execute(object parameter)
        {
            OnExecuting();
            Method((T)parameter);
            OnExecuted();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler CanExecuteChanged;
        public event EventHandler Executing;
        public event EventHandler Executed;
    }


}
