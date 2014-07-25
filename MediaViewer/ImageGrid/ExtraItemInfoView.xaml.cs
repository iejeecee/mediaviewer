using MediaViewer.MediaFileModel.Watcher;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ExtraImageGridItemInfoView.xaml
    /// </summary>
    public partial class ExtraItemInfoView : UserControl
    {
        public ExtraItemInfoView()
        {
            InitializeComponent();
        }
               
        public SortMode InfoType
        {
            get { return (SortMode)GetValue(InfoTypeProperty); }
            set { SetValue(InfoTypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InfoType.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InfoTypeProperty =
            DependencyProperty.Register("InfoType", typeof(SortMode), typeof(ExtraItemInfoView), new PropertyMetadata(SortMode.Name, extraImageGridItemInfoView_InfoTypeChangedCallback));

        private static void extraImageGridItemInfoView_InfoTypeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ExtraItemInfoView view = d as ExtraItemInfoView;
            SortMode infoType = (SortMode)e.NewValue;
            MediaFileItem item = view.DataContext as MediaFileItem;

            String info = "";

            if (item.Media != null)
            {
                switch (infoType)
                {
                    case SortMode.Name:
                        break;
                    case SortMode.Size:
                        info = Utils.Misc.formatSizeBytes(item.Media.SizeBytes);
                        break;
                    case SortMode.Rating:

                        if(item.Media.Rating.HasValue) {
                            view.rating.Value = item.Media.Rating.Value / 5;
                            view.rating.Visibility = Visibility.Visible;
                        }
                        break;
                    case SortMode.Imported:
                        break;
                    case SortMode.Tags:
                        if (item.Media.Tags.Count > 0)
                        {
                            info = item.Media.Tags.Count.ToString() + " tag";

                            if (item.Media.Tags.Count > 1)
                            {
                                info += "s";
                            }
                        }
                        break;
                    case SortMode.CreationDate:
                        if (item.Media.CreationDate.HasValue)
                        {
                            info = item.Media.CreationDate.Value.ToString("MMM d, yyyy");
                        }
                        break;
                    default:
                        break;
                }
            }

            view.infoTextBlock.Text = info;

            if (infoType != SortMode.Rating)
            {
                view.rating.Visibility = Visibility.Hidden;
                view.infoTextBlock.Visibility = Visibility.Visible;
            }
            else
            {          
                view.infoTextBlock.Visibility = Visibility.Hidden;
            }
           
        }

    }
}
