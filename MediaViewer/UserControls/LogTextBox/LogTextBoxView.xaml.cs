using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.UserControls.LogTextBox
{
    /// <summary>
    /// Interaction logic for LogTextBoxView.xaml
    /// </summary>
    public partial class LogTextBoxView : UserControl
    {
        public LogTextBoxView()
        {
            InitializeComponent();
        }

        public ObservableCollection<String> InfoMessages
        {
            get { return (ObservableCollection<String>)GetValue(InfoMessagesProperty); }
            set { SetValue(InfoMessagesProperty, value); }
        }

        public static readonly DependencyProperty InfoMessagesProperty =
            DependencyProperty.Register("InfoMessages", typeof(ObservableCollection<String>), typeof(LogTextBoxView), new PropertyMetadata(null, new PropertyChangedCallback(infoMessagesChangedCallback)));

        static void infoMessagesChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            LogTextBoxView control = (LogTextBoxView)o;

            if (e.OldValue != null)
            {
                var coll = (ObservableCollection<String>)e.OldValue;
                // Unsubscribe from CollectionChanged on the old collection
                coll.CollectionChanged -= control.infoMessages_CollectionChanged;
            }

            if (e.NewValue != null)
            {
                var coll = (ObservableCollection<String>)e.NewValue;
                // Subscribe to CollectionChanged on the new collection
                control.addText(coll);

                coll.CollectionChanged += control.infoMessages_CollectionChanged;
              
            }

        }

        private void infoMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {          
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        addText(e.NewItems);                   
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        reset(e.NewItems);                 
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
           
        }

        void addText(IList text)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (String message in text)
                {
                    infoTextBox.AppendText(message + "\n");
                }

                infoTextBox.ScrollToEnd();

            }));
        }

        void reset(IList text)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                infoTextBox.Text = "";
                if (text == null) return;

                foreach (String message in text)
                {
                    infoTextBox.AppendText(message + "\n");
                }
                infoTextBox.ScrollToEnd();

            }));

        }
    }
}
