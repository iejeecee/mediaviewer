using MediaViewer.Infrastructure.Utils;
using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Global.Events;
using MediaViewer.Model.Media.Base.Item;
using MediaViewer.Model.Utils;
using MediaViewer.UserControls.TabbedExpander;
using Microsoft.Practices.Prism.PubSubEvents;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Windows.Themes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading;
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
using System.Windows.Threading;
using VideoLib;

namespace MediaViewer.UserControls.MediaPreview
{
    /// <summary>
    /// Interaction logic for MediaPreviewView.xaml
    /// </summary>
    [Export, PartCreationPolicy(CreationPolicy.NonShared)]
    public partial class MediaPreviewView : UserControl, INavigationAware, ITabbedExpanderAware
    {
        MediaPreviewViewModelBase ViewModel { get; set; }
        TimeAdorner TimeAdorner { get; set; }
        DispatcherTimer Timer { get; set; }

        CancellationTokenSource TokenSource { get; set; }
        SemaphoreSlim semaphore;
        SemaphoreSlim Semaphore {
            get
            {
                return semaphore;
            }
            set
            {
                semaphore = value;
            }
        }

        int nrWaitingThreads;
                
        [ImportingConstructor]
        public MediaPreviewView()
        {
            InitializeComponent();
          
            TabName = "Preview";
            TabIsSelected = true;          
            TabMargin = new Thickness(2);
            TabBorderThickness = new Thickness(2);
            TabBorderBrush = ClassicBorderDecorator.ClassicBorderBrush;
           
            TimeAdorner = new TimeAdorner(previewImage);
                  
            Timer = new DispatcherTimer();
            Timer.Tick += Timer_Elapsed;
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 5);

            nrWaitingThreads = 0;
            Semaphore = new SemaphoreSlim(1);
        }

        async void Timer_Elapsed(object sender, EventArgs e)
        {
            Timer.Stop();
           
            Point mousePos = Mouse.GetPosition(previewImage);
         
            double position = mousePos.X / previewImage.ActualWidth;

            Interlocked.Increment(ref nrWaitingThreads);

            try
            {
                MediaThumb thumbnail = await Task<MediaThumb>.Factory.StartNew(() =>
                {
                    Semaphore.Wait();

                    Interlocked.Decrement(ref nrWaitingThreads);
                  
                    if (nrWaitingThreads > 0 || TokenSource.IsCancellationRequested)
                    {
                        return (null);
                    }

                    return ViewModel.getVideoPreviewThumbnail(position, TokenSource.Token);

                });

                if (thumbnail == null) return;

                previewImage.Source = thumbnail.Thumb;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);

                adornerLayer.Remove(TimeAdorner);

                Size size = TimeAdorner.Size;
                double xLeft = mousePos.X - size.Width / 2;
                double xRight = xLeft + size.Width;

                if (xLeft < 0) xLeft = 0;
                if (xRight > previewImage.ActualWidth) xLeft = previewImage.ActualWidth - size.Width;

                TimeAdorner.Location = new Point(xLeft, previewImage.ActualHeight - size.Height);
                TimeAdorner.TimeSeconds = (int)thumbnail.PositionSeconds;

                adornerLayer.Add(TimeAdorner);
            }
            catch (Exception)
            {
               
            }
            finally
            {
                Semaphore.Release();
            }
        }
               

        public string TabName { get; set; }
        public bool TabIsSelected { get; set; }
        public Thickness TabMargin { get; set; }
        public Thickness TabBorderThickness { get; set; }
        public Brush TabBorderBrush { get; set; }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
           
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            ViewModel = navigationContext.Parameters["MediaPreviewViewModel"] as MediaPreviewViewModelBase;

            if(ViewModel == null) throw new ArgumentNullException("Missing MediaPreviewViewModel in navigationContext parameters");

            ViewModel.PropertyChanged -= viewModel_PropertyChanged;
            ViewModel.PropertyChanged += viewModel_PropertyChanged;
        }

        private async void viewModel_PropertyChanged(Object sender, PropertyChangedEventArgs e)
        {            
            await App.Current.Dispatcher.BeginInvoke(new Action(async () => {

                await close(); 
             
                if (ViewModel.MediaPreviewImage == null)
                {
                    previewImageBorder.Visibility = Visibility.Collapsed;       
                    previewImage.Source = null;             
               
                    return;
                }

                previewImageBorder.Visibility = Visibility.Visible;
                previewImage.Source = ViewModel.MediaPreviewImage;

                BitmapImage errorImage = (ViewModel.MediaPreviewImage as BitmapImage);

                if (errorImage != null && 
                    errorImage.UriSource != null && 
                    errorImage.UriSource.ToString().EndsWith("error.png"))
                {
                    previewImage.Stretch = Stretch.None;
                
                } else {

                    previewImage.Stretch = Stretch.Uniform;
                }
                       
            }));
        }
      
        private async void previewImage_MouseLeave(object sender, MouseEventArgs e)
        {
            await close();
        }

        async Task close()
        {
            if (TokenSource != null)
            {
                TokenSource.Cancel();
            }

            await Semaphore.WaitAsync();
            try
            {
                ViewModel.endVideoPreview();

                previewImage.Source = ViewModel.MediaPreviewImage;

                AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(this);
                if (adornerLayer != null)
                {
                    adornerLayer.Remove(TimeAdorner);
                }
            }
            finally
            {
                Semaphore.Release();
            }
        }

      
        private void previewImage_previewMouseMove(object sender, MouseEventArgs e)
        {
            Timer.Stop();
            Timer.Start();            
            
        }

        private async void previewImage_MouseEnter(object sender, MouseEventArgs e)
        {            
            loadingView.VisibilityAndAnimate = Visibility.Visible;
            TokenSource = new CancellationTokenSource();

            try
            {
                await Task.Factory.StartNew(() =>
                {
                    Semaphore.Wait();
                    try
                    {
                        ViewModel.startVideoPreview(TokenSource.Token);
                    }                 
                    finally
                    {
                        Semaphore.Release();
                    }

                }, TokenSource.Token);

            }
            catch (Exception)
            {

            }
                        
            loadingView.VisibilityAndAnimate = Visibility.Collapsed;
        }
                    
    }
}
