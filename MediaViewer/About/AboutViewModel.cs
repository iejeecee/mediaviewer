using MvvmFoundation.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MediaViewer.About
{
    class AboutViewModel : ObservableObject
    {

        public AboutViewModel()
        {
            AssemblyInfo = Assembly.GetEntryAssembly().GetName();           
        }

        AssemblyName assemblyInfo;

        public AssemblyName AssemblyInfo
        {
            get { return assemblyInfo; }
            set
            {
                assemblyInfo = value;
                NotifyPropertyChanged();
            }
        }
    }
}
