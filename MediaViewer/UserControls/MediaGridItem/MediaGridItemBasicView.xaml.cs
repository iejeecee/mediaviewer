using MediaViewer.Model.Media.File;
using MediaViewer.Model.Global.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using MediaViewer.Model.Utils;
using MediaViewer.Model.Media.State.CollectionView;
using Microsoft.Practices.Prism.PubSubEvents;
using System.ComponentModel.Composition;
using Microsoft.Practices.ServiceLocation;
using MediaViewer.Model.Media.Base;

namespace MediaViewer.UserControls.MediaGridItem
{
    /// <summary>
    /// Interaction logic for MediaGridItemBasicView.xaml
    /// </summary>
    public partial class MediaGridItemBasicView : UserControl
    {
        public event EventHandler<SelectableMediaItem> Click;
        public event EventHandler<SelectableMediaItem> Checked;
        public event EventHandler<SelectableMediaItem> Unchecked;

        IEventAggregator EventAggregator { get; set; }

        public MediaGridItemBasicView()
        {
            InitializeComponent();

            EventAggregator = ServiceLocator.Current.GetInstance(typeof(IEventAggregator)) as IEventAggregator;
        }

        public SelectableMediaItem SelectableMediaItem
        {
            get { return (SelectableMediaItem)GetValue(SelectableMediaItemProperty); }
            set { SetValue(SelectableMediaItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SelectableMediaFileItem.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectableMediaItemProperty =
            DependencyProperty.Register("SelectableMediaItem", typeof(SelectableMediaItem), typeof(MediaGridItemBasicView), new PropertyMetadata(null));

        private void viewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (Click != null)
            {
                Click(this, SelectableMediaItem);
            }

        }
                
        private void imageGridItem_Checked(object sender, RoutedEventArgs e)
        {
            if (Checked != null)
            {
                Checked(this, SelectableMediaItem);
            }
                     
        }

        private void imageGridItem_Unchecked(object sender, RoutedEventArgs e)
        {
            if (Unchecked != null)
            {
                Unchecked(this, SelectableMediaItem);
            }
                     
        }
          
    }
}
