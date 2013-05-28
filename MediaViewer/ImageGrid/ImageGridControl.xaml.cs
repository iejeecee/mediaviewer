using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using MediaViewer.MediaPreview;
using MediaViewer.Utils.WPF;

namespace MediaViewer.ImageGrid
{
    /// <summary>
    /// Interaction logic for ImageGridControl.xaml
    /// </summary>
    public partial class ImageGridControl : UserControl, INotifyPropertyChanged
    {

        public event EventHandler<EventArgs> UpdateImages;

        TrulyObservableCollection<ImagePanelItem> panelItems;
        List<MediaPreviewAsyncState> imageItems;
        int columns;

        public int Columns
        {
            get { return columns; }
            set
            {
                columns = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Columns"));
                }
            }
        }
        int rows;

        public int Rows
        {
            get { return rows; }
            set
            {
                rows = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Rows"));
                }
            }
        }
        int currentPage;

        public ImageGridControl()
        {

            createGrid(5, 5);
           
            InitializeComponent();

           

            currentPage = 0;

            columns = 0;
            rows = 0;

            panelItems = null;
            imageItems = null;

        }

        public TrulyObservableCollection<ImagePanelItem> PanelItems
        {
            get
            {
                return (panelItems);
            }
            set
            {
                panelItems = value;
            }
        }

        public int CurrentPage
        {
            get
            {
                return (currentPage);
            }
        }

        public int NrImages
        {
            get
            {
                if (imageItems == null) return (0);
                return (imageItems.Count);
            }
        }

        public int NrPages
        {
            get
            {

                int nrPages = (int)Math.Ceiling((double)NrImages / NrPanels);

                return (nrPages);
            }

        }

        public int NrPanels
        {
            get
            {
                return (columns * rows);
            }
        }

        public void createGrid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;

            panelItems = new TrulyObservableCollection<ImagePanelItem>();
            for (int i = 0; i < NrPanels; i++)
            {
                panelItems.Add(new ImagePanelItem());
            }

            this.DataContext = PanelItems;
           

            /*
            panel = new MediaPreviewControl[NrPanels];

            Grid grid = new Grid();
            grid.HorizontalAlignment = HorizontalAlignment.Stretch;
            grid.VerticalAlignment = VerticalAlignment.Stretch;

            for (int i = 0; i < columns; i++)
            {
                ColumnDefinition column = new ColumnDefinition();

                grid.ColumnDefinitions.Add(column);
            }

            for (int i = 0; i < rows; i++)
            {
                RowDefinition row = new RowDefinition();

                grid.RowDefinitions.Add(row);
            }

            for (int i = 0; i < NrPanels; i++)
            {
                // border

                Border border = new Border();
                border.BorderThickness = new Thickness(1);
                border.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 0));

                int column = i % columns;
                int row = i / rows;

                Grid.SetRow(border, row);
                Grid.SetColumn(border, column);

                grid.Children.Add(border);

                // preview

                MediaPreviewControl preview = new MediaPreviewControl();
                preview.MouseLeftButtonDown += new MouseButtonEventHandler(mediaPreview_LeftMouseButtonDown);
                preview.PanelIndex = i;
                panel[i] = preview;

                preview.HorizontalAlignment = HorizontalAlignment.Stretch;
                preview.VerticalAlignment = VerticalAlignment.Stretch;

                Grid.SetRow(preview, row);
                Grid.SetColumn(preview, column);

                grid.Children.Add(preview);

            }

            mainGrid.Children.Clear();
            mainGrid.Children.Add(grid);
             */
        }

        int panelToImageNr(int panelNr)
        {

            return (CurrentPage * NrPanels + panelNr);

        }

        int imageToPanelNr(int imageNr)
        {

            if (imageNr - CurrentPage * NrPanels < 0)
            {

                return (-1);

            }
            else if (imageNr - CurrentPage * NrPanels < NrPanels)
            {

                return (imageNr - CurrentPage * NrPanels);

            }
            else
            {

                return (-1);
            }
        }



        void displayImage(int panelNr, MediaPreviewAsyncState imageItem)
        {
          
          

            panelItems[panelNr].AsyncState = imageItem;

        }


        public void initializeImageData(List<MediaPreviewAsyncState> imageData)
        {

            this.imageItems = imageData;

            displayPage(0);

        }

        public void addImageData(MediaPreviewAsyncState addData)
        {

            List<MediaPreviewAsyncState> addDataList = new List<MediaPreviewAsyncState>();

            addDataList.Add(addData);

            addImageData(addDataList);
        }

        public void addImageData(List<MediaPreviewAsyncState> addData)
        {

            if (imageItems == null)
            {

                imageItems = new List<MediaPreviewAsyncState>();
            }

            for (int i = 0; i < addData.Count; i++)
            {

                imageItems.Add(addData[i]);
                int panelNr = imageToPanelNr(NrImages - 1);

                if (panelNr != -1)
                {

                    displayImage(panelNr, addData[i]);
                }

                UpdateImages(this, EventArgs.Empty);
            }


        }

        public void removeAllImageData()
        {

            List<MediaPreviewAsyncState> emptyList = new List<MediaPreviewAsyncState>();

            initializeImageData(emptyList);
        }

        public void removeImageData(MediaPreviewAsyncState removeData)
        {

            if (removeData == null) return;

            List<MediaPreviewAsyncState> removeDataList = new List<MediaPreviewAsyncState>();

            removeDataList.Add(removeData);

            removeImageData(removeDataList);
        }

        public void removeImageData(List<MediaPreviewAsyncState> removeData)
        {

            if (removeData == null) return;

            int startPage = CurrentPage;

            for (int j = 0; j < removeData.Count; j++)
            {

                for (int i = 0; i < NrImages; i++)
                {

                    if (removeData[j].MediaLocation.Equals(imageItems[i].MediaLocation))
                    {

                        imageItems.RemoveAt(i);
                        break;
                    }
                }
            }

            while (startPage >= NrPages)
            {

                if (startPage == 0) break;
                else --startPage;
            }


            displayPage(startPage);

        }

        public void replaceImageData(int imageNr, MediaPreviewAsyncState updateData)
        {

            System.Diagnostics.Debug.Assert(imageNr < NrImages);

            imageItems[imageNr] = updateData;

            int panelNr = imageToPanelNr(imageNr);

            if (panelNr != -1)
            {

                displayImage(panelNr, updateData);
            }
        }

        public void updateImageData(MediaPreviewAsyncState updateData)
        {

            List<MediaPreviewAsyncState> updateDataList = new List<MediaPreviewAsyncState>();

            updateDataList.Add(updateData);

            updateImageData(updateDataList);
        }

        public void updateImageData(List<MediaPreviewAsyncState> updateData)
        {

            for (int i = 0; i < NrImages; i++)
            {

                for (int j = 0; j < updateData.Count; j++)
                {

                    if (imageItems[i].MediaLocation.Equals(updateData[j].MediaLocation))
                    {

                        imageItems[i] = updateData[j];
                    }
                }
            }

            // only have to redraw image if it's on the current page
            for (int i = 0; i < NrPanels && panelToImageNr(i) < NrImages; i++)
            {

                int k = panelToImageNr(i);

                for (int j = 0; j < updateData.Count; j++)
                {

                    if (imageItems[k].MediaLocation.Equals(updateData[j].MediaLocation))
                    {

                        displayImage(i, updateData[j]);
                    }
                }
            }
        }
        public MediaPreviewAsyncState getImageData(string imageLocation)
        {

            for (int i = 0; i < NrImages; i++)
            {

                if (imageItems[i].MediaLocation.Equals(imageLocation))
                {

                    return (imageItems[i]);
                }

            }

            return (null);
        }
        public MediaPreviewAsyncState getImageData(int imageNr)
        {

            return (imageItems[imageNr]);
        }

        public List<MediaPreviewAsyncState> getSelectedImageData()
        {

            List<MediaPreviewAsyncState> selectedImageData = new List<MediaPreviewAsyncState>();

            for (int i = 0; i < NrImages; i++)
            {

                if (imageItems[i].IsSelected)
                {

                    selectedImageData.Add(imageItems[i]);
                }
            }

            return (selectedImageData);
        }






        public bool displayNextPage()
        {

            if (currentPage + 1 < NrPages)
            {

                displayPage(currentPage + 1);
                UpdateImages(this, EventArgs.Empty);
                return (true);

            }
            else
            {

                return (false);
            }
        }

        public bool displayPrevPage()
        {

            if (currentPage - 1 < 0)
            {

                return (false);

            }
            else
            {

                displayPage(currentPage - 1);
                UpdateImages(this, EventArgs.Empty);
                return (true);
            }

        }


        public void displayPage(int page)
        {

            if (imageItems == null) return;

            currentPage = page;

            for (int panelNr = 0; panelNr < NrPanels; panelNr++)
            {

                int imageNr = panelToImageNr(panelNr);

                if (imageNr < NrImages)
                {

                    // when changes occur rapidly this check bugs out (fix?)
                    //if(!panel[panelNr].Location.Equals(imageData[imageNr].ImageLocation)) {

                    // only update the image if it is different from the
                    // currently displayed image
                    displayImage(panelNr, imageItems[imageNr]);
                    //}

                }
                else
                {                  
                    panelItems[panelNr].AsyncState = MediaPreviewAsyncState.Empty;                 
                }

            }

            UpdateImages(this, EventArgs.Empty);
        }

        public bool isImageSelected(int imageNr)
        {

            Debug.Assert(imageNr < NrImages);

            return (imageItems[imageNr].IsSelected);
        }

        public void setImageSelected(int imageNr, bool mode)
        {

            Debug.Assert(imageNr < NrImages);

            if (isImageSelected(imageNr) == mode) return;
            else toggleImageSelected(imageNr);
        }

        public void toggleImageSelected(int imageNr)
        {

            Debug.Assert(imageNr < NrImages);

            if (imageItems[imageNr].IsSelected == true)
            {

                imageItems[imageNr].IsSelected = false;

            }
            else
            {

                imageItems[imageNr].IsSelected = true;
            }

            int panelNr = imageToPanelNr(imageNr);

            if (panelNr >= 0)
            {

                panelItems[panelNr].IsSelected = imageItems[imageNr].IsSelected;

            }

        }

        public void setSelectedForAllImages(bool isSelected)
        {

            for (int i = 0; i < NrImages; i++)
            {

                if (imageItems[i].IsSelected != isSelected)
                {

                    toggleImageSelected(i);
                }
            }

        }

        private void mediaPreview_LeftMouseButtonDown(object sender, MouseButtonEventArgs e)
        {
            MediaPreviewControl preview = (MediaPreviewControl)e.Source;

            //toggleImageSelected(panelToImageNr(preview.BindHelper));
        }


        public bool isEmpty(int panelNr)
        {

            return (panelItems[panelNr].AsyncState.IsEmpty ? true : false);
        }


        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
