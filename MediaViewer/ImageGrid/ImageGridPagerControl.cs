using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediaViewer.Pager;

namespace MediaViewer.ImageGrid
{
    class ImageGridPagerControl : PagerControl
    {
        ImageGridControl imageGrid;

        public ImageGridPagerControl()
        {

            imageGrid = null;
            BeginButtonClick += new EventHandler<EventArgs>(beginPage_Click);
            PrevButtonClick += new EventHandler<EventArgs>(prevPage_Click);
            NextButtonClick += new EventHandler<EventArgs>(nextPage_Click);
            EndButtonClick += new EventHandler<EventArgs>(endPage_Click);
        }

        public ImageGridControl ImageGrid
        {

            get
            {

                return (imageGrid);
            }

            set
            {

                this.imageGrid = value;

                if (imageGrid != null)
                {

                    imageGrid.UpdateImages += new EventHandler<EventArgs>(imageGrid_UpdateImages);
                }
            }
        }


        private void beginPage_Click(System.Object sender, System.EventArgs e)
        {

            imageGrid.displayPage(0);
        }
        private void prevPage_Click(System.Object sender, System.EventArgs e)
        {

            imageGrid.displayPrevPage();
        }
        private void nextPage_Click(System.Object sender, System.EventArgs e)
        {

            imageGrid.displayNextPage();
        }
        private void endPage_Click(System.Object sender, System.EventArgs e)
        {

            imageGrid.displayPage(imageGrid.NrPages - 1);
        }

        private void imageGrid_UpdateImages(System.Object sender, System.EventArgs e)
        {

            if (imageGrid.CurrentPage == 0)
            {

                PrevButtonEnabled = false;
                BeginButtonEnabled = false;

            }
            else
            {

                PrevButtonEnabled = true;
                BeginButtonEnabled = true;
            }


            if (imageGrid.CurrentPage >= imageGrid.NrPages - 1)
            {

                NextButtonEnabled = false;
                EndButtonEnabled = false;

            }
            else
            {

                NextButtonEnabled = true;
                EndButtonEnabled = true;
            }

            int curPage = imageGrid.NrPages > 0 ? imageGrid.CurrentPage + 1 : 0;

            CurrentPage = curPage;
            TotalPages = imageGrid.NrPages;

        }
		
    }
}
