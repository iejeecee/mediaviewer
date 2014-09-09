using MediaViewer.Model.Media.File;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MediaViewer.ImageGrid
{
    public class ImageGridMouseEventArgs : RoutedEventArgs
    {

        MouseEventArgs mouseEvent;

        public MouseEventArgs MouseEvent
        {
            get { return mouseEvent; }
            set { mouseEvent = value; }
        }

        int panelNr;

        public int PanelNr
        {
            get { return panelNr; }
            set { panelNr = value; }
        }
        int imageNr;

        public int ImageNr
        {
            get { return imageNr; }
            set { imageNr = value; }
        }
        int row;

        public int Row
        {
            get { return row; }
            set { row = value; }
        }
        int column;

        public int Column
        {
            get { return column; }
            set { column = value; }
        }
        MediaFileItem item;

        public MediaFileItem Item
        {
            get { return item; }
            set { item = value; }
        }


    public ImageGridMouseEventArgs(MouseEventArgs mouseEvent, int panelNr, int imageNr, int row,
        int column, MediaFileItem item) 	
	{
        MouseEvent = mouseEvent;
		PanelNr = panelNr;
		Row = row;
		Column = column;
		Item = item;
		ImageNr = imageNr;
	}





    }
}
