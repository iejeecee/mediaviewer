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
    public class Command : DelegateCommand, INotifyPropertyChanged
    {
             
        public event EventHandler Executing;
        public event EventHandler<Task> Executed;

        public Command(Action executeMethod, bool isExecuteable = true)
            : base(executeMethod)
        {
            this.isExecuteable = isExecuteable;

            //Hack to get IsExecutable working correctly
            Func<Object, bool> canExecute = new Func<Object, bool>((o) => {return(IsExecutable);});

            typeof(Command).GetField("_canExecuteMethod", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, canExecute);
        }

        public override Task Execute()
        {
            OnExecuting();

            return base.Execute().ContinueWith((result) => OnExecuted(result));
        }

        bool isExecuteable;

        public bool IsExecutable
        {
            get { return isExecuteable; }
            set {

                if (isExecuteable == value) return;
                isExecuteable = value;
               
                RaiseCanExecuteChanged();
                OnPropertyChanged("IsExecutable");
            }
        }
 
        protected void OnExecuting() {

            if (Executing != null)
            {
                Executing(this, EventArgs.Empty);
            }
        }

        protected void OnExecuted(Task result)
        {
            if (Executed != null)
            {
                Executed(this, result);
            }
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Command<T> : DelegateCommand<T>, INotifyPropertyChanged
    {
        public event EventHandler Executing;
        public event EventHandler<Task> Executed;

        public Command(Action<T> executeMethod, bool isExecuteable = true)
            : base(executeMethod)
        {
            this.isExecuteable = isExecuteable;

            Func<Object, bool> canExecute = new Func<Object, bool>((o) => { return (IsExecutable); });

            typeof(Command<T>).GetField("_canExecuteMethod", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, canExecute);
        }

        public override Task Execute(T arg)
        {
            OnExecuting();

            return base.Execute(arg).ContinueWith((result) => OnExecuted(result));
        }

        public override bool CanExecute(T value)
        {
            return (isExecuteable);
        }

        bool isExecuteable;

        public bool IsExecutable
        {
            get { return isExecuteable; }
            set
            {
                if (isExecuteable == value) return;
                isExecuteable = value;

                RaiseCanExecuteChanged();
                OnPropertyChanged("IsExecutable");
            }
        }

        protected void OnExecuting()
        {

            if (Executing != null)
            {
                Executing(this, EventArgs.Empty);
            }
        }

        protected void OnExecuted(Task result)
        {
            if (Executed != null)
            {
                Executed(this, result);
            }
        }

        protected void OnPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

    }


}
