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
    public class AutoCompleteBoxViewModel : ObservableObject
    {
        TernaryTree tree;

        public AutoCompleteBoxViewModel()
        {
            Suggestions = new ObservableRangeCollection<Object>();
            Items = null;
            CustomFindMatchesFunction = null;

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

        public delegate List<Object> CustomFindMatchesDelegate(String text);
        CustomFindMatchesDelegate customFindMatchesFunction;

        public CustomFindMatchesDelegate CustomFindMatchesFunction
        {
            get { return customFindMatchesFunction; }
            set { customFindMatchesFunction = value; }
        }

        private void findSuggestions()
        {
            
            if (String.IsNullOrEmpty(Text))
            {
                Suggestions.Clear();
                return;
            }

            List<Object> matches;

            if (CustomFindMatchesFunction != null)
            {
                matches = CustomFindMatchesFunction(Text);
            }
            else
            {
                matches = tree.AutoComplete(Text);
            }
          
            Suggestions.ReplaceRange(matches.Take(Math.Min(MaxSuggestions, matches.Count)));
                                      
        }
    }
}
