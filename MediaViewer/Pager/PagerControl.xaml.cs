using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace MediaViewer.Pager
{
    /// <summary>
    /// Interaction logic for PagerControl.xaml
    /// </summary>
    public partial class PagerControl : UserControl, INotifyPropertyChanged
    {
        public PagerControl()
        {
            InitializeComponent();
            CurrentPage = 0;
            TotalPages = 0;
        }

        public event EventHandler<EventArgs> BeginButtonClick;
        public event EventHandler<EventArgs> EndButtonClick;
        public event EventHandler<EventArgs> NextButtonClick;
        public event EventHandler<EventArgs> PrevButtonClick;

        int currentPage;

        public int CurrentPage
        {

            get
            {

                return (currentPage);
            }

            set
            {
                if (value < 0)
                {
                    currentPage = 0;
                }
                else if (value > TotalPages)
                {
                    currentPage = TotalPages;
                }
                else
                {
                    currentPage = value;
                }
                
                PropertyChanged(this, new PropertyChangedEventArgs("CurrentPage"));

            }
        }

        int totalPages;

        public int TotalPages
        {

            get
            {
                return (totalPages);
            }

            set
            {

                totalPages = value;
                PropertyChanged(this, new PropertyChangedEventArgs("TotalPages"));

            }
        }

        public bool NextButtonEnabled
        {

            get
            {

                return (nextButton.IsEnabled);
            }

            set
            {

                nextButton.IsEnabled = value;
            }
        }

        public bool PrevButtonEnabled
        {

            get
            {

                return (prevButton.IsEnabled);
            }

            set
            {

                prevButton.IsEnabled = value;
            }
        }

        public bool BeginButtonEnabled
        {

            get
            {

                return (beginButton.IsEnabled);
            }

            set
            {

                beginButton.IsEnabled = value;
            }
        }

        public bool EndButtonEnabled
        {

            get
            {

                return (endButton.IsEnabled);
            }

            set
            {

                endButton.IsEnabled = value;
            }
        }

        private void nextButton_Click(object sender, RoutedEventArgs e)
        {

            NextButtonClick(this, e);
        }
        private void endButton_Click(object sender, RoutedEventArgs e)
        {

            EndButtonClick(this, e);
        }
        private void prevButton_Click(object sender, RoutedEventArgs e)
        {

            PrevButtonClick(this, e);
        }
     

        private void beginButton_Click(object sender, RoutedEventArgs e)
        {
            BeginButtonClick(this, e);
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
