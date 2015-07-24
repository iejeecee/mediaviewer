using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.SortComboBox
{
   
    public abstract class SortItemBase<T> : BindableBase
    {
        public event EventHandler SortDirectionChanged;
        public Command ToggleDirectionCommand { get; set; }

        public SortItemBase(T mode)
        {
            this.sortMode = mode;
            sortDirection = ListSortDirection.Ascending;

            ToggleDirectionCommand = new Command(() =>
            {
                if (SortDirection == ListSortDirection.Ascending)
                {
                    SortDirection = ListSortDirection.Descending;
                }
                else
                {
                    SortDirection = ListSortDirection.Ascending;
                }
            });
        }

        T sortMode;

        public T SortMode
        {
            get { return sortMode; }
            set { sortMode = value;
                SetProperty(ref sortMode, value);
            }
        }

        ListSortDirection sortDirection;

        public ListSortDirection SortDirection
        {
            get { return sortDirection; }
            set { 
                
                sortDirection = value;
                OnPropertyChanged("SortDirection");

                if (SortDirectionChanged != null)
                {
                    SortDirectionChanged(this, EventArgs.Empty);
                }
            }
        }

    }
}
