using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.AutoCompleteBox
{
    interface IAutoCompleteBoxViewModel
    {
        
        Object SelectedObject
        {
            get;
         
        }
      
        ObservableRangeCollection<Object> Suggestions
        {
            get;
            set;
        }
      
        IEnumerable<Object> Items
        {
            get;
            set;
        }
      
        String Text
        {
            get;
            set;
        }
      
        Command<Object> SetSuggestionCommand
        {
            get;
            set;
        }
     
        int MaxSuggestions
        {
            get;
            set;
        }

        
    }
}
