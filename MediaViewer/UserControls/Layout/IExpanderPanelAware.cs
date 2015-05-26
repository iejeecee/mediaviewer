using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.Layout
{
    public interface IExpanderPanelAware
    {
        String Header { get; set; }
        int ElementHeight { get; set; }
        bool IsAddBorder { get; set; }
        bool IsIntiallyExpanded { get; set; }
    }
}
