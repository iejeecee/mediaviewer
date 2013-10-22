using MvvmFoundation.Wpf;
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
    public partial class LogView : Window
    {
      
        public LogView()
        {
            InitializeComponent();

            trimChars = new char[1];
            trimChars[0] = '\n';

            logTextBox.Document.PageWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
			//filterComboBox.SelectedIndex = 2;

            DataContextChanged += new DependencyPropertyChangedEventHandler(logView_DataContextChanged);
            Loaded += new RoutedEventHandler((o, e) =>
            {              
                logTextBox.ScrollToEnd();                
            });

        }

        void logView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null) return;

            LogViewModel logViewModel = (LogViewModel)e.NewValue;

            foreach (LogMessageModel message in logViewModel.Messages)
            {
                addMessage(message);
            }
          
            logViewModel.Messages.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler((s, target) =>
                {
                    if (target.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                    {
                        logTextBox.Document.Blocks.Clear();
                    }
                    else if (target.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        foreach (LogMessageModel message in target.NewItems)
                        {
                            addMessage(message);
                        }
                      
                    }

                });           
            
        }

        private char[] trimChars;

        private MediaViewer.Logging.LogMessageModel.LogLevel filterLevel;

        private void addMessage(LogMessageModel message)
        {
            if ((int)message.Level > (int)filterLevel)
            {
                return;
            }

            String text = message.Text.TrimEnd(trimChars);
            text = text.Replace("\r\n", "\n");
            text = text.Replace("\n", "\r\t");
           
            
            TextRange tr = new TextRange(logTextBox.Document.ContentEnd,
            logTextBox.Document.ContentEnd);

            tr.Text = text;

            SolidColorBrush brush;

            switch (message.Level)
            {

                case MediaViewer.Logging.LogMessageModel.LogLevel.UNKNOWN:
                    {

                        brush = Brushes.Black;
                        break;
                    }
                case MediaViewer.Logging.LogMessageModel.LogLevel.DEBUG:
                    {
                        brush = Brushes.LightBlue;
                        break;
                    }
                case MediaViewer.Logging.LogMessageModel.LogLevel.ERROR:
                    {
                        brush = Brushes.Red;
                        break;
                    }
                case MediaViewer.Logging.LogMessageModel.LogLevel.FATAL:
                    {
                        brush = Brushes.DarkRed;
                        break;
                    }
                case MediaViewer.Logging.LogMessageModel.LogLevel.INFO:
                    {
                        brush = Brushes.Blue;
                        break;
                    }
                case MediaViewer.Logging.LogMessageModel.LogLevel.WARNING:
                    {
                        brush = Brushes.Orange;
                        break;
                    }
                default:
                    {
                        brush = Brushes.Black;
                        break;
                    }

            }

            tr.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
  

            logTextBox.ScrollToEnd();
            
        }

    
        private void filterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            if (filterComboBox.SelectedIndex == 0)
            {

                filterLevel = MediaViewer.Logging.LogMessageModel.LogLevel.ERROR;

            }
            else if (filterComboBox.SelectedIndex == 1)
            {

                filterLevel = MediaViewer.Logging.LogMessageModel.LogLevel.WARNING;

            }
            else if (filterComboBox.SelectedIndex == 2)
            {

                filterLevel = MediaViewer.Logging.LogMessageModel.LogLevel.INFO;
            }
            else
            {
                filterLevel = MediaViewer.Logging.LogMessageModel.LogLevel.DEBUG;
            }
        }

      
    }
}
