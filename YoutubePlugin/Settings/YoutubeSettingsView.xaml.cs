using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
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

namespace YoutubePlugin.Settings
{
    /// <summary>
    /// Interaction logic for YoutubeSettingsView.xaml
    /// </summary>
    [Export]
    public partial class YoutubeSettingsView : UserControl
    {
        [ImportingConstructor]
        public YoutubeSettingsView(YoutubeSettingsViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }
    }
}
