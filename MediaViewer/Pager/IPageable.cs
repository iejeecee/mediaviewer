using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Pager
{
    public interface IPageable
    {
        int NrPages { get; set; }
        int CurrentPage { get; set; }
        bool IsPagingEnabled { get; set; }
        Command NextPageCommand { get; set; }
        Command PrevPageCommand { get; set; }
        Command FirstPageCommand { get; set; }
        Command LastPageCommand { get; set; }
    }
}
