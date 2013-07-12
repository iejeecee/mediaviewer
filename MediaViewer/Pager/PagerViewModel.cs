using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.Pager
{
    class PagerViewModel : ObservableObject
    {
      

        public PagerViewModel()
        {
            nextPageCommand = new Command(new Action(() =>
            {
                CurrentPage += 1;

               
            }));

            prevPageCommand = new Command(new Action(() =>
            {
                CurrentPage -= 1;

             
            }));

            firstPageCommand = new Command(new Action(() =>
            {
                CurrentPage = 0;

              
            }));

            lastPageCommand = new Command(new Action(() =>
            {
                CurrentPage = TotalPages;

             
            }));

            TotalPages = 1;
            CurrentPage = 1;
        }

        int totalPages;

        public int TotalPages
        {
            get
            {
                return (totalPages);
            }

            set
            {
                totalPages = value;

                if (currentPage > TotalPages)
                {
                    currentPage = TotalPages;
                }

                setExecuteState();

                NotifyPropertyChanged();
            }
        }

        int currentPage;

        public int CurrentPage
        {

            get
            {
                return (currentPage);
            }

            set
            {
                if (value < 1)
                {
                    currentPage = 1;
                }
                else if (value > TotalPages)
                {
                    currentPage = TotalPages;
                }
                else
                {
                    currentPage = value;
                }

                setExecuteState();

                NotifyPropertyChanged();

            }
        }

        void setExecuteState()
        {
            if (CurrentPage + 1 <= TotalPages)
            {
                nextPageCommand.CanExecute = true;
                lastPageCommand.CanExecute = true;
            }
            else
            {
                nextPageCommand.CanExecute = false;
                lastPageCommand.CanExecute = false;
            }

            if (CurrentPage - 1 >= 1)
            {
                prevPageCommand.CanExecute = true;
                firstPageCommand.CanExecute = true;
            }
            else
            {
                prevPageCommand.CanExecute = false;
                firstPageCommand.CanExecute = false;
            }
        }

        Command nextPageCommand;

        public Command NextPageCommand
        {
            get { return nextPageCommand; }
            set { nextPageCommand = value; }
        }
        Command prevPageCommand;

        public Command PrevPageCommand
        {
            get { return prevPageCommand; }
            set { prevPageCommand = value; }
        }
        Command firstPageCommand;

        public Command FirstPageCommand
        {
            get { return firstPageCommand; }
            set { firstPageCommand = value; }
        }
        Command lastPageCommand;

        public Command LastPageCommand
        {
            get { return lastPageCommand; }
            set { lastPageCommand = value; }
        }

      
    }
}
