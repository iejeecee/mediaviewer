using MediaViewer.Utils;
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
using System.Windows.Shapes;

namespace MediaViewer.MoveRename
{
    /// <summary>
    /// Interaction logic for MoveRenameProgressWindow.xaml
    /// </summary>
    public partial class MoveRenameProgressWindow : Window
    {
        FileUtilsProgress progress;

        public MoveRenameProgressWindow()
        {
            InitializeComponent();

            progress = new FileUtilsProgress();
            DataContext = progress;

            progress.ClosingRequest += new EventHandler((o, e) => this.Close());

            Closing += new System.ComponentModel.CancelEventHandler((s, e) =>
            {
                progress.CancelCommand.DoExecute();
            });

        }
    }
}
