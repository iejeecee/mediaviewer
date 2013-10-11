using System;
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

namespace MediaViewer.Progress
{
    /// <summary>
    /// Interaction logic for ProgressControl.xaml
    /// </summary>
    public partial class ProgressControl : UserControl
    {
        public ProgressControl()
        {
            InitializeComponent();
            okButton.IsEnabled = false;
       
        }

        public int TotalProgress
        {
            get { return (int)GetValue(TotalProgressProperty); }
            set { SetValue(TotalProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalProgressProperty =
            DependencyProperty.Register("TotalProgress", typeof(int), typeof(ProgressControl), new PropertyMetadata(0, new PropertyChangedCallback(totalProgressChangedCallback)));


        public int TotalProgressMax
        {
            get { return (int)GetValue(TotalProgressMaxProperty); }
            set { SetValue(TotalProgressMaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalProgressMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalProgressMaxProperty =
            DependencyProperty.Register("TotalProgressMax", typeof(int), typeof(ProgressControl), new PropertyMetadata(0, new PropertyChangedCallback(totalProgressMaxChangedCallback)));

        public int ItemProgress
        {
            get { return (int)GetValue(ItemProgressProperty); }
            set { SetValue(ItemProgressProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemProgress.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProgressProperty =
            DependencyProperty.Register("ItemProgress", typeof(int), typeof(ProgressControl), new PropertyMetadata(0, new PropertyChangedCallback(itemProgressChangedCallback)));

        public int ItemProgressMax
        {
            get { return (int)GetValue(ItemProgressMaxProperty); }
            set { SetValue(ItemProgressMaxProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemProgressMax.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemProgressMaxProperty =
            DependencyProperty.Register("ItemProgressMax", typeof(int), typeof(ProgressControl), new PropertyMetadata(100, new PropertyChangedCallback(itemProgressMaxChangedCallback)));


        public string ItemInfo
        {
            get { return (string)GetValue(ItemInfoProperty); }
            set { SetValue(ItemInfoProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemInfo.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemInfoProperty =
            DependencyProperty.Register("ItemInfo", typeof(string), typeof(ProgressControl), new PropertyMetadata(String.Empty, new PropertyChangedCallback(itemInfoChangedCallback)));
       
        static void itemInfoChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;

            String infoText = (String)e.NewValue;

            control.itemLabel.Content = infoText;
            control.itemLabel.ToolTip = infoText;

        }

        public ObservableCollection<String> InfoMessages
        {
            get { return (ObservableCollection<String>)GetValue(ItemInfoProperty); }
            set { SetValue(ItemInfoProperty, value); }
        }

        public static readonly DependencyProperty InfoMessagesProperty =
            DependencyProperty.Register("InfoMessages", typeof(ObservableCollection<String>), typeof(ProgressControl), new PropertyMetadata(null, new PropertyChangedCallback(infoMessagesChangedCallback)));

        static void infoMessagesChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;

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
                coll.CollectionChanged += control.infoMessages_CollectionChanged;
            }

        }

        private void infoMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        {
                            foreach (String message in e.NewItems)
                            {

                                infoTextBox.Text += message + "\n";
                            }

                            break;
                        }
                    case NotifyCollectionChangedAction.Reset:
                        {
                            infoTextBox.Text = "";
                            foreach (String message in e.NewItems)
                            {

                                infoTextBox.Text += message + "\n";
                            }
                            break;
                        }
                    default:
                        {
                            break;
                        }
                }
            }));
        }

        static void totalProgressChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;

            String content = "Finished: " + ((int)e.NewValue).ToString() + " / " + control.TotalProgressMax.ToString();

            control.progressLabel.Content = content;
            control.totalProgressBar.Value = (double)(int)e.NewValue;

            if (control.totalProgressBar.Value == control.TotalProgressMax)
            {
                control.okButton.IsEnabled = true;
            }

        }

        static void totalProgressMaxChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;

            String content = "Finished: " + control.TotalProgress.ToString() + " / " + ((int)e.NewValue).ToString();

            control.progressLabel.Content = content;
            control.totalProgressBar.Maximum = (double)(int)e.NewValue;
        }

        static void itemProgressChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;

            control.itemProgressBar.Value = (double)(int)e.NewValue;
        }

        static void itemProgressMaxChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;
        
            control.itemProgressBar.Maximum = (double)(int)e.NewValue;
        }

        // http://msdn.microsoft.com/en-us/library/system.windows.input.icommandsource.command.aspx
        public ICommand CancelCommand
        {
            get { return (ICommand)GetValue(CancelProperty); }
            set { SetValue(CancelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Cancel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CancelProperty =
            DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(ProgressControl), new PropertyMetadata(null, new PropertyChangedCallback(cancelCommandChangedCallback)));

        static void cancelCommandChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;

            control.cancelButton.Command = (ICommand)e.NewValue;
        }

        public ICommand OkCommand
        {
            get { return (ICommand)GetValue(OkCommandProperty); }
            set { SetValue(OkCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for OkCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OkCommandProperty =
            DependencyProperty.Register("OkCommand", typeof(ICommand), typeof(ProgressControl), new PropertyMetadata(null, new PropertyChangedCallback(okCommandChangedCallback)));

        static void okCommandChangedCallback(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ProgressControl control = (ProgressControl)o;

            control.okButton.Command = (ICommand)e.NewValue;
        }
    
    }
}
