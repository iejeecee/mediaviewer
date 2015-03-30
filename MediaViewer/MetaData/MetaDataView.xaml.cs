using MediaViewer.Model.Media.File;
using MediaViewer.Model.Media.File.Watcher;
using MediaViewer.MetaData.Tree;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using MediaViewer.ExtensionMethods;
using System.Windows.Threading;
using System.ComponentModel.Composition;
using MediaViewer.Model.Settings;
using Microsoft.Practices.Prism.Regions;
using Microsoft.Practices.Prism.PubSubEvents;

namespace MediaViewer.MetaData
{
    /// <summary>
    /// Interaction logic for MetaDataView.xaml
    /// </summary>
    [Export]
    public partial class MetaDataView : UserControl, IRegionMemberLifetime, INavigationAware
    {

        MetaDataViewModel MetaDataViewModel { get; set; }     
        IEventAggregator EventAggregator { get; set; }
       
        [ImportingConstructor]
        public MetaDataView(IEventAggregator eventAggregator)
        {
            InitializeComponent();

            EventAggregator = eventAggregator;

            MetaDataViewModel = new MetaDataViewModel(MediaFileWatcher.Instance, AppSettings.Instance, eventAggregator);
                                     
            dynamicElements = new List<UIElement>();
            dynamicRows = new List<RowDefinition>();

            DataContext = MetaDataViewModel;

         
        }
                  
        List<RowDefinition> dynamicRows;
        List<UIElement> dynamicElements;

        void displayDynamicProperties(List<Tuple<String, String>> additionalProps)
        {
           /* App.Current.Dispatcher.BeginInvoke(new Action(() =>
            {
                foreach (RowDefinition row in dynamicRows)
                {
                    miscGrid.RowDefinitions.Remove(row);
                }

                dynamicRows.Clear();

                foreach (UIElement elem in dynamicElements)
                {
                    miscGrid.Children.Remove(elem);
                }

                dynamicElements.Clear();

                foreach (Tuple<String, String> prop in additionalProps)
                {

                    RowDefinition row;

                    if (String.IsNullOrEmpty(prop.Item1))
                    {
                        row = new RowDefinition();
                        row.Height = GridLength.Auto;

                        miscGrid.RowDefinitions.Add(row);
                        dynamicRows.Add(row);

                        Separator seperator = new Separator();
                        dynamicElements.Add(seperator);

                        miscGrid.Children.Add(seperator);
                        Grid.SetRow(seperator, miscGrid.RowDefinitions.Count - 1);
                        Grid.SetColumnSpan(seperator, 3);

                    }

                    row = new RowDefinition();
                    row.Height = GridLength.Auto;

                    miscGrid.RowDefinitions.Add(row);
                    dynamicRows.Add(row);

                    Label label = new Label();
                    label.Style = Resources["labelStyle"] as Style;
                    label.Content = prop.Item1;

                    miscGrid.Children.Add(label);
                    dynamicElements.Add(label);
                    Grid.SetColumn(label, 0);
                    Grid.SetRow(label, miscGrid.RowDefinitions.Count - 1);

                    TextBlock value = new TextBlock();
                    value.Text = prop.Item2;
                    value.Margin = new Thickness(5, 5, 5, 5);

                    miscGrid.Children.Add(value);
                    dynamicElements.Add(value);
                    Grid.SetColumn(value, 1);
                    Grid.SetRow(value, miscGrid.RowDefinitions.Count - 1);

                }
            }));*/
        }

        private void fileNameContextMenu_InsertCounter(object sender, RoutedEventArgs e)
        {
            TextBox textBox = fileNameTextBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            }

            int index = textBox.CaretIndex;

            MetaDataViewModel.InsertCounterCommand.Execute(index);
        }

        private void fileNameContextMenu_InsertExistingFilename(object sender, RoutedEventArgs e)
        {
            TextBox textBox = fileNameTextBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            }

            int index = textBox.CaretIndex;

            MetaDataViewModel.InsertExistingFilenameCommand.Execute(index);
        }

        private void fileNameContextMenu_InsertResolution(object sender, RoutedEventArgs e)
        {
            TextBox textBox = fileNameTextBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            }

            int index = textBox.CaretIndex;

            MetaDataViewModel.InsertResolutionCommand.Execute(index);

        }

        private void fileNameContextMenu_InsertDate(object sender, RoutedEventArgs e)
        {
            TextBox textBox = fileNameTextBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            }

            int index = textBox.CaretIndex;

            MetaDataViewModel.InsertDateCommand.Execute(index);

        }

        private void fileNameContextMenu_InsertReplaceString(object sender, RoutedEventArgs e)
        {
            TextBox textBox = fileNameTextBox.getChildrenOfType<TextBox>().
                       FirstOrDefault(element => element.Name == "PART_EditableTextBox");

            if (textBox == null)
            {
                return;
            }

            int index = textBox.CaretIndex;

            MetaDataViewModel.InsertReplaceStringCommand.Execute(index);
        }

        public bool KeepAlive
        {
            get { return (true); }
        }


        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return (true);
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            MetaDataViewModel.OnNavigatedFrom(navigationContext);           
        }        

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            MetaDataViewModel.OnNavigatedTo(navigationContext);            
        }
        
    }
}
