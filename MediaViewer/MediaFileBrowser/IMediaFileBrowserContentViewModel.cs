using Microsoft.Practices.Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.MediaFileBrowser
{
    public interface IMediaFileBrowserContentViewModel
    {
        void OnNavigatedTo(NavigationContext navigationContext);
        void OnNavigatedFrom(NavigationContext navigationContext);
    }
}
