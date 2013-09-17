using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// In order to use a pagerview, add IPageable interface to viewmodel and set the pagerview datacontext to this viewmodel

namespace MediaViewer.Pager
{
    interface IPageable
    {

        bool IsPagingEnabled
        {
            get;
            set;            
        }

        int NrPages
        {
            get;
            set;
        }

        int CurrentPage
        {
            get;
            set;
        }

        Command NextPageCommand
        {
            get;
            set;
        }
        Command PrevPageCommand
        {
            get;
            set;
        }
        Command FirstPageCommand
        {
            get;
            set;
        }
        Command LastPageCommand
        {
            get;
            set;
        }
    }
}
