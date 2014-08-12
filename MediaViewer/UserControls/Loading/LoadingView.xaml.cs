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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.UserControls.Loading
{
    /// <summary>
    /// Interaction logic for LoadingView.xaml
    /// </summary>
    public partial class LoadingView : UserControl
    {
        bool isAnimationRunning;

        public LoadingView()
        {
            InitializeComponent();

            isAnimationRunning = false;
        }

        public Visibility VisibilityAndAnimate
        {
            get { return (Visibility)GetValue(VisibilityAndAnimateProperty); }
            set { SetValue(VisibilityAndAnimateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for VisibilityAndAnimate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VisibilityAndAnimateProperty =
            DependencyProperty.Register("VisibilityAndAnimate", typeof(Visibility), typeof(LoadingView), new PropertyMetadata(Visibility.Hidden,visibilityAndAnimateChangedCallback));

        private static void visibilityAndAnimateChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            LoadingView loadingView = (LoadingView)d;

            Visibility newVisibility = (Visibility)e.NewValue;

            Storyboard animation = (Storyboard)loadingView.Resources["Storyboard1"];

            if (newVisibility == Visibility.Visible)
            {
                animation.Begin();
                loadingView.isAnimationRunning = true;
            }
            else
            {
                if (loadingView.isAnimationRunning == true)
                {
                    animation.Stop();
                    loadingView.isAnimationRunning = false;
                }
               
            }

            loadingView.Visibility = newVisibility;
        }        
    }
}
