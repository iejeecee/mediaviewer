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

namespace MediaViewer.UserControls.Pager
{
    /// <summary>
    /// Interaction logic for PagerControl.xaml
    /// </summary>
    public partial class PagerView : UserControl
    {
        public PagerView()
        {
            InitializeComponent();
           
        }
       
        private void currentPageTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {              
                BindingExpression exp = GetBindingExpression(CurrentPageProperty);

                if (exp != null)
                {                 
                    int pageNr;
                    bool success = int.TryParse(currentPageTextBox.Text, out pageNr);

                    if (success)
                    {
                        CurrentPage = pageNr;
                        exp.UpdateSource();
                    }                
                }                               
            }
        }

        public bool IsPagingEnabled
        {
            get { return (bool)GetValue(IsPagingEnabledProperty); }
            set { SetValue(IsPagingEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsPagingEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsPagingEnabledProperty =
            DependencyProperty.Register("IsPagingEnabled", typeof(bool), typeof(PagerView), new PropertyMetadata(false, new PropertyChangedCallback(pagingEnabledChanged)));

        private static void pagingEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagerView pagerView = (PagerView)d;
            pagerView.mainGrid.IsEnabled = (bool)e.NewValue;
        }

        public int NrPages
        {
            get { return (int)GetValue(NrPagesProperty); }
            set { SetValue(NrPagesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NrPages.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NrPagesProperty =
            DependencyProperty.Register("NrPages", typeof(int), typeof(PagerView), new PropertyMetadata(0, new PropertyChangedCallback(nrPagesChanged)));

        private static void nrPagesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagerView pagerView = (PagerView)d;
            pagerView.totalPagesTextBox.Text = ((int)e.NewValue).ToString();
        }

        public Nullable<int> CurrentPage
        {
            get { return (Nullable<int>)GetValue(CurrentPageProperty); }
            set { SetValue(CurrentPageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentPage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageProperty =
            DependencyProperty.Register("CurrentPage", typeof(Nullable<int>), typeof(PagerView), 
             new FrameworkPropertyMetadata() {
                BindsTwoWayByDefault = true,
                DefaultUpdateSourceTrigger = UpdateSourceTrigger.Explicit,
                PropertyChangedCallback = new PropertyChangedCallback(currentPageChanged)                
               }, new ValidateValueCallback(validateCurrentPage)        
            );

        private static bool validateCurrentPage(object value)
        {
            if (value == null || value.GetType() == typeof(int))
            {
                return (true);

            } else {

                return (false);
            }
        }

        private static void currentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagerView pagerView = (PagerView)d;

            Nullable<int> newValue = (Nullable<int>)e.NewValue;

            if (newValue == null)
            {
                pagerView.currentPageTextBox.Text = "";
            }
            else
            {
                pagerView.currentPageTextBox.Text = (newValue.Value).ToString();
            }
        }

        public ICommand NextPageCommand
        {
            get { return (ICommand)GetValue(NextPageCommandProperty); }
            set { SetValue(NextPageCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NextPageCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NextPageCommandProperty =
            DependencyProperty.Register("NextPageCommand", typeof(ICommand), typeof(PagerView), new PropertyMetadata(null, new PropertyChangedCallback(nextPageCommandChanged)));

        private static void nextPageCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagerView pagerView = (PagerView)d;
            pagerView.nextButton.Command = (ICommand)e.NewValue;
        }

        public ICommand PrevPageCommand
        {
            get { return (ICommand)GetValue(PrevPageCommandProperty); }
            set { SetValue(PrevPageCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PrevPageCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrevPageCommandProperty =
            DependencyProperty.Register("PrevPageCommand", typeof(ICommand), typeof(PagerView), new PropertyMetadata(null, new PropertyChangedCallback(prevPageCommandChanged)));

        private static void prevPageCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagerView pagerView = (PagerView)d;
            pagerView.prevButton.Command = (ICommand)e.NewValue;
        }


        public ICommand FirstPageCommand
        {
            get { return (ICommand)GetValue(FirstPageCommandProperty); }
            set { SetValue(FirstPageCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FirstPageCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FirstPageCommandProperty =
            DependencyProperty.Register("FirstPageCommand", typeof(ICommand), typeof(PagerView), new PropertyMetadata(null, new PropertyChangedCallback(firstPageCommandChanged)));

        private static void firstPageCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagerView pagerView = (PagerView)d;
            pagerView.beginButton.Command = (ICommand)e.NewValue;
            
        }

        public ICommand LastPageCommand
        {
            get { return (ICommand)GetValue(LastPageCommandProperty); }
            set { SetValue(LastPageCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LastPageCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LastPageCommandProperty =
            DependencyProperty.Register("LastPageCommand", typeof(ICommand), typeof(PagerView), new PropertyMetadata(null, new PropertyChangedCallback(lastPageCommandChanged)));

        private static void lastPageCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PagerView pagerView = (PagerView)d;
            pagerView.endButton.Command = (ICommand)e.NewValue;
        }   


    }
}
