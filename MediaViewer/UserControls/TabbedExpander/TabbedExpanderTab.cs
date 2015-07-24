using Microsoft.Windows.Themes;
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

namespace MediaViewer.UserControls.TabbedExpander
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MediaViewer.UserControls.TabbedExpander"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:MediaViewer.UserControls.TabbedExpander;assembly=MediaViewer.UserControls.TabbedExpander"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:TabbedExpanderTab/>
    ///
    /// </summary>
    public class TabbedExpanderTab : ContentControl
    {
        static TabbedExpanderTab()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabbedExpanderTab), new FrameworkPropertyMetadata(typeof(TabbedExpanderTab)));
            
        }

        public string TabName
        {
            get { return (string)GetValue(TabNameProperty); }
            set { SetValue(TabNameProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TabName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TabNameProperty =
            DependencyProperty.Register("TabName", typeof(string), typeof(TabbedExpanderTab), new PropertyMetadata(null));


        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TabbedExpanderTab), new PropertyMetadata(false));

        public ClassicBorderStyle BorderStyle
        {
            get { return (ClassicBorderStyle)GetValue(BorderStyleProperty); }
            set { SetValue(BorderStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderStyleProperty =
            DependencyProperty.Register("BorderStyle", typeof(ClassicBorderStyle), typeof(TabbedExpanderTab), new PropertyMetadata(ClassicBorderStyle.Etched));
        
    }
}
