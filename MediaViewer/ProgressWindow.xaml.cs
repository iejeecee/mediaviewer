using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MediaViewer
{
    ////// <summary>
    ////// Interaction logic for ProgressWindow.xaml
    ////// <//summary>
    public partial class ProgressWindow : Window
    {
        private Object userState;
        private delegate void setDoubleDelegate(double value);
        private delegate void setStringDelegate(String value);
        protected delegate void asyncAddInfoStringDelegate(String info);
        protected bool abortAsyncAction;

        public delegate void CancelEventHandler(Object sender, EventArgs e);
        public event CancelEventHandler CancelEvent;

        public ProgressWindow()
        {
            InitializeComponent();
        }

        protected virtual void asyncAction(Object state)
        {

        }

        public void addInfoString(string info)
        {
            if (Dispatcher.CheckAccess())
            {
                asyncAddInfoStringInvoke(info);
            }
            else
            {
                Object[] infoArgs = new Object[1];

                infoArgs[0] = info;
                asyncAddInfoStringDelegate addInfo = new asyncAddInfoStringDelegate(this, &ProgressForm.asyncAddInfoStringInvoke);

                Dispatcher.BeginInvoke(addInfo, infoArgs);
            }

        }

        private void asyncAddInfoStringInvoke(string info)
        {
            infoTextBox.Text = infoTextBox.Text + info + "\r\n\r\n";
            infoTextBox.SelectionStart = infoTextBox.Text.Length;
            infoTextBox.ScrollToEnd();
        }

        public void actionFinished()
        {
            if (Dispatcher.CheckAccess())
            {
                asyncActionFinishedInvoke();
            }
            else
            {
                Dispatcher.BeginInvoke(new Action(asyncActionFinishedInvoke));
            }
        }
        private void asyncActionFinishedInvoke()
        {
            okButton.IsEnabled = true;
            cancelButton.IsEnabled = false;
        }

        public Object UserState
        {
            set
            {
                userState = value;
            }

            get
            {
                return (userState);
            }
        }

        public double TotalProgressMaximum
        {
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    setTotalProgressMaximum(value);
                }
                else
                {
                    Object[] args = new Object[1];
                    args[0] = value;

                    Dispatcher.BeginInvoke(new setDoubleDelegate(setTotalProgressMaximum), args);
                }
            }

            get
            {
                return (totalProgressBar.Maximum);
            }
        }

        private void setTotalProgressMaximum(double maximum)
        {
            totalProgressBar.Maximum = maximum;
        }

        public double ItemProgressMaximum
        {
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    setItemProgressMaximum(value);
                }
                else
                {
                    Object[] args = new Object[1];
                    args[0] = value;

                    Dispatcher.BeginInvoke(new setDoubleDelegate(setItemProgressMaximum), args);
                }
            }

            get
            {
                return (itemProgressBar.Maximum);
            }
        }
        private void setItemProgressMaximum(double maximum)
        {
            itemProgressBar.Maximum = maximum;
        }

        public double ItemProgressValue
        {
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    setItemProgressValue(value);
                }
                else
                {
                    Object[] args = new Object[1];
                    args[0] = value;

                    Dispatcher.BeginInvoke(new setDoubleDelegate(setItemProgressValue), args);
                }
            }

            get
            {
                return (itemProgressBar.Value);
            }
        }
        private void setItemProgressValue(double value)
        {
            itemProgressBar.Value = value;
        }

        public double TotalProgressValue
        {
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    setTotalProgressValue(value);
                }
                else
                {
                    Object[] args = new Object[1];
                    args[0] = value;

                    Dispatcher.BeginInvoke(new setDoubleDelegate(setTotalProgressValue), args);
                }
            }

            get
            {
                return (totalProgressBar.Value);
            }
        }
        private void setTotalProgressValue(double value)
        {
            progressLabel.Content = "Finished: " + Convert.ToString(value) + " // " + Convert.ToString(totalProgressBar.Maximum);
            totalProgressBar.Value = value;
        }

        public string ItemInfo
        {
            set
            {
                if (Dispatcher.CheckAccess())
                {
                    setItemInfo(value);
                }
                else
                {
                    Object[] args = new Object[1];
                    args[0] = value;

                    Dispatcher.BeginInvoke(new setStringDelegate(setItemInfo), args);
                }
            }

            get
            {

                return ((string)itemLabel.Content);
            }
        }

        private void setItemInfo(string value)
        {

            itemLabel.Content = value;
            ////itemLabel.ContentStringFormat = System.Drawing.ContentAlignment.MiddleCenter;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {

            abortAsyncAction = true;
            CancelEvent(this, EventArgs.Empty);
        }
    }
}
