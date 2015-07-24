using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MediaViewer.UserControls.TabbedExpander
{
    public interface ITabbedExpanderAware 
    {
        String TabName { get;} 
        bool TabIsSelected { get;}
        Thickness TabMargin { get; }
        Thickness TabBorderThickness { get; }
        Brush TabBorderBrush { get; }
    }
}
