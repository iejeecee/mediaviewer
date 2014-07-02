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

namespace MediaViewer.Progress
{
    /// <summary>
    /// Interaction logic for CancellableOperationProgressView.xaml
    /// </summary>
    public partial class CancellableOperationProgressView : Window
    {
        public CancellableOperationProgressView()
        {
            InitializeComponent();

            DataContextChanged += CancellableOperationProgressView_DataContextChanged;

            Closing += new System.ComponentModel.CancelEventHandler((s, e) =>
            {
                (DataContext as ICancellableOperationProgress).CancelCommand.DoExecute();
            });
        }

        private void CancellableOperationProgressView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null) 
            {
                (e.OldValue as MvvmFoundation.Wpf.CloseableObservableObject).ClosingRequest -= CloseableObservableObject_ClosingRequest;
            }

            if (e.NewValue != null)
            {
                (e.NewValue as MvvmFoundation.Wpf.CloseableObservableObject).ClosingRequest += CloseableObservableObject_ClosingRequest;
            }           
        }

        private void CloseableObservableObject_ClosingRequest(object sender, EventArgs e)
        {
            this.Close();
        }            
    }
}
