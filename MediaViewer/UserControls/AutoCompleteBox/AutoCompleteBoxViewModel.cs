using MediaViewer.MediaFileModel.Watcher;
using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.AutoCompleteBox
{
    class AutoCompleteBoxViewModel : ObservableObject, IAutoCompleteBoxViewModel
    {
        TernaryTree tree;

        public AutoCompleteBoxViewModel()
        {
            Suggestions = new ObservableRangeCollection<Object>();
            Items = null;

            SetSuggestionCommand = new Command<Object>(new Action<Object>((item) => {

                Text = item.ToString();
                SelectedObject = item;
            }));

            tree = new TernaryTree();
            MaxSuggestions = 50;
        }

        Object selectedObject;

        public Object SelectedObject
        {
            get { return selectedObject; }
            private set { selectedObject = value;
            NotifyPropertyChanged();
            }
        }

        ObservableRangeCollection<Object> suggestions;

        public ObservableRangeCollection<Object> Suggestions
        {
            get { return suggestions; }
            set { suggestions = value;
            NotifyPropertyChanged();
            }
        }

        IEnumerable<Object> items;

        public IEnumerable<Object> Items
        {
            get { return items; }
            set
            {
                items = value;

                if (items != null)
                {
                    tree.Clear();

                    foreach (Object item in items)
                    {
                        tree.Add(item);
                    }              
                }
            }
        }

        String text;

        public String Text
        {
            get { return text; }
            set { text = value;
            findSuggestions();
            NotifyPropertyChanged();
            }
        }

        Command<Object> setSuggestionCommand;

        public Command<Object> SetSuggestionCommand
        {
            get { return setSuggestionCommand; }
            set { setSuggestionCommand = value;
            NotifyPropertyChanged();
            }
        }

        int maxSuggestions;

        public int MaxSuggestions
        {
            get { return maxSuggestions; }
            set { maxSuggestions = value;
            NotifyPropertyChanged();
            }
        }

        private void findSuggestions()
        {
            
            if (String.IsNullOrEmpty(Text) || Items == null)
            {
                Suggestions.Clear();
                return;
            }

            List<Object> matches = tree.AutoComplete(Text);
          
            Suggestions.ReplaceRange(matches.Take(Math.Min(MaxSuggestions, matches.Count)));
                                      
        }
    }
}
