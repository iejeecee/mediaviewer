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
            getLibraryVersionsInfo();
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

        String libraryVersionsInfo;

        public String LibraryVersionsInfo
        {
            get { return libraryVersionsInfo; }
            set { libraryVersionsInfo = value;
            NotifyPropertyChanged();
            }
        }

        void getLibraryVersionsInfo()
        {
            XMPLib.VersionInfo info = new XMPLib.VersionInfo();
            XMPLib.MetaData.getVersionInfo(ref info);

            StringBuilder sb = new StringBuilder();

            String debug = info.isDebug ? "(Debug)" : "";

            sb.AppendLine("XMPLib Version: " + info.major + "." + info.minor + "." + info.micro + " " + debug);
            sb.AppendLine(info.message);

            int version = VideoLib.VideoPlayer.getAvFormatVersion();
            int major = version >> 16;
            int minor = (version >> 8) & 0xFF;
            int micro = version & 0xFF;

            sb.AppendLine("FFmpeg Version: " + major + "." + minor + "." + micro);
            sb.AppendLine();
            sb.AppendLine("Warning this computer program is protected by copyright law and international treaties.");
            sb.AppendLine("Unathorized reproduction or distribution of this program, or any portion of it, may result in severe criminal penalties, and will be prosecuted to the maximum extent possible under the law.");
            
            LibraryVersionsInfo = sb.ToString();
        }
    }
}
