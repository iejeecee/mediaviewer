using MediaViewer.MediaDatabase;
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

namespace MediaViewer.MetaData
{
    /// <summary>
    /// Interaction logic for LinkedTagEditorView.xaml
    /// </summary>
    public partial class LinkedTagEditorView : Window
    {
        LinkedTagEditorViewModel linkedTagEditorViewModel;

        public LinkedTagEditorView()
        {
            InitializeComponent();
            DataContext = linkedTagEditorViewModel = new LinkedTagEditorViewModel();

            linkedTagEditorViewModel.ClearTagCommand.Executed += new MvvmFoundation.Wpf.Delegates.CommandEventHandler((o, e) =>
            {

                addChildTagTextBox.Text = "";
            });

            linkedTagEditorViewModel.AddChildTagCommand.Executed += new MvvmFoundation.Wpf.Delegates.CommandEventHandler((o, e) =>
            {
                addChildTagTextBox.Text = "";
            });


        }
    }
}
