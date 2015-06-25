using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MediaViewer.Model.Mvvm
{

    class AsyncCommand : AsyncCommand<object>
    {
        public AsyncCommand(Func<Task> method, bool isExecutable = true)
            : base(o => method(), isExecutable)
        {

        }

    }

    class AsyncCommand<T> : ICommand, INotifyPropertyChanged
    {
        Func<T,Task> Method { get; set; }

        public AsyncCommand(Func<T,Task> method, bool isExecutable = true)
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
            set {

                if (value == isExecutable) return;

                isExecutable = value;
                OnPropertyChanged("IsExecutable");
                OnCanExecuteChanged();
            }
        }

        public bool CanExecute(object parameter)
        {
            return (IsExecutable);
        }
        
        public async Task ExecuteAsync(object parameter)
        {
            OnExecuting();
            await Method((T)parameter);          
            OnExecuted();
        }

        public async void Execute(object parameter)
        {
            OnExecuting();
            await Method((T)parameter);
            OnExecuted();
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

        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler Executing;
        public event EventHandler Executed;
        public event EventHandler CanExecuteChanged;
    }
}
