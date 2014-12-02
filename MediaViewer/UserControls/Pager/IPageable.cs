using MediaViewer.Model.Mvvm;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.UserControls.Pager
{
    public interface IPageable
    {
        int NrPages { get; set; }
        Nullable<int> CurrentPage { get; set; }
        bool IsPagingEnabled { get; set; }
        Command NextPageCommand { get; set; }
        Command PrevPageCommand { get; set; }
        Command FirstPageCommand { get; set; }
        Command LastPageCommand { get; set; }
    }
}
