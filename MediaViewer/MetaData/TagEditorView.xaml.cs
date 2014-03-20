using MediaViewer.MediaDatabase;
using MediaViewer.MediaDatabase.DbCommands;
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
    public partial class TagEditorView : Window
    {
        TagEditorViewModel tagEditorViewModel;

        public TagEditorView()
        {
            InitializeComponent();
            DataContext = tagEditorViewModel = new TagEditorViewModel();

            categoryNameAutoCompleteBox.CustomFindMatchesFunction = new UserControls.AutoCompleteBox.AutoCompleteBoxView.CustomFindMatchesDelegate((text) =>
            {
                List<TagCategory> results = new List<TagCategory>();

                using (TagCategoryDbCommands tc = new TagCategoryDbCommands())
                {
                    results = tc.getCategoryAutocompleteMatches(text);
                }

                return (results.Cast<Object>().ToList());
            });

            tagNameAutoCompleteBox.CustomFindMatchesFunction = 
                new UserControls.AutoCompleteBox.AutoCompleteBoxView.CustomFindMatchesDelegate(tagAutoComplete);           
        }

        List<Object> tagAutoComplete(String text)
        {
            List<Tag> results = new List<Tag>();

            using (TagDbCommands tc = new TagDbCommands())
            {
                results = tc.getTagAutocompleteMatches(text);
            }

            return (results.Cast<Object>().ToList());
        }
    }
}
