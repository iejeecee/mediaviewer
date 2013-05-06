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

namespace MediaViewer.Logging
{
    /// <summary>
    /// Interaction logic for LogWindow.xaml
    /// </summary>
    public partial class LogWindow : Window
    {

        public enum LogLevel
        {
            UNKNOWN,
            FATAL,
            ERROR,
            WARNING,
            INFO,
            DEBUG
        }

        public LogWindow()
        {
            InitializeComponent();
        }

        private delegate void AddTextDelegate(LogLevel level, string text);
        private AddTextDelegate addTextDelegate;
        private Object[] args;

        private LogLevel filterLevel;

        private void addText(LogLevel level, string text)
        {

            TextRange tr = new TextRange(logTextBox.Document.ContentEnd,
            logTextBox.Document.ContentEnd);

            tr.Text = text;

            SolidColorBrush brush;

            switch (level)
            {

                case LogLevel.UNKNOWN:
                    {

                        brush = Brushes.Yellow;
                        break;
                    }
                case LogLevel.DEBUG:
                    {
                        brush = Brushes.LightBlue;
                        break;
                    }
                case LogLevel.ERROR:
                    {
                        brush = Brushes.Red;
                        break;
                    }
                case LogLevel.FATAL:
                    {
                        brush = Brushes.DarkRed;
                        break;
                    }
                case LogLevel.INFO:
                    {
                        brush = Brushes.Blue;
                        break;
                    }
                case LogLevel.WARNING:
                    {
                        brush = Brushes.Yellow;
                        break;
                    }

            }

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, Brushes.Red);

            logTextBox.ScrollToEnd();
        }

        public void append(LogLevel level, string text)
        {

            if (filterLevel < level) return;

            lock (args)
            {

                args[0] = level;
                args[1] = text;

                if (!this.IsInitialized)
                {
                    this.BeginInit();
                }

                if (Dispatcher.CheckAccess())
                {
                    addText(level, text);

                }
                else
                {

                    Dispatcher.BeginInvoke(addTextDelegate, args);
                }

            }

        }

        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            logTextBox.Document.Blocks.Clear();
        }

        private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (filterComboBox.SelectedIndex == 0)
            {

                filterLevel = LogLevel.INFO;

            }
            else if (filterComboBox.SelectedIndex == 1)
            {

                filterLevel = LogLevel.WARNING;

            }
            else
            {

                filterLevel = LogLevel.ERROR;
            }
        }

        /*
   
                private System.Void LogForm_FormClosing(System.Object sender, System.Windows.Forms.FormClosingEventArgs e)
                {
                    e.Cancel = true;
                    this.Hide();
                }
         */
    }
}
