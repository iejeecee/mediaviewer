using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.PubSubEvents;
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

namespace MediaViewer.TagEditor
{
    /// <summary>
    /// Interaction logic for LinkedTagEditorView.xaml
    /// </summary>
    public partial class TagEditorView : Window
    {
        TagEditorViewModel tagEditorViewModel;

        IEventAggregator EventAggregator { get; set; }

        public TagEditorView(IEventAggregator eventAggregator)
        {
            EventAggregator = eventAggregator;

            InitializeComponent();
            DataContext = tagEditorViewModel = new TagEditorViewModel(eventAggregator);

            tagEditorViewModel.ClosingRequest += new EventHandler<CloseableBindableBase.DialogEventArgs>((s, e) =>
            {                
                this.Close();
            });

            tagEditorViewModel.ImportCommand.Executed += (s,e) =>
            {
              //  tagTreePicker.reloadAll();
            };
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //tagTreePicker.unregisterMessages();          
        }
       
    }
}
