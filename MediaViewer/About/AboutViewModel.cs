using MediaViewer.Model.Settings;
using Microsoft.Practices.Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;
using MediaViewer.Model.Mvvm;
using MediaViewer.Logging;

namespace MediaViewer.About
{
    [Export]
    public class AboutViewModel : SettingsBase
    {
        public Command LogCommand { get; set; }

        public AboutViewModel() :
            base("About", new Uri(typeof(AboutView).FullName, UriKind.Relative))
        {
            AssemblyInfo = Assembly.GetEntryAssembly().GetName();       
            getLibraryVersionsInfo();

            LogCommand = new Command(() =>
            {
                LogCommand.IsExecutable = false;

                LogView logView = new LogView();
                logView.Closed += logView_Closed;
                logView.Show();
            });
        }

        void logView_Closed(object sender, EventArgs e)
        {
            LogCommand.IsExecutable = true;
        }

        AssemblyName assemblyInfo;

        public AssemblyName AssemblyInfo
        {
            get { return assemblyInfo; }
            set
            {
                SetProperty(ref assemblyInfo, value);              
            }
        }

        String libraryVersionsInfo;

        public String LibraryVersionsInfo
        {
            get { return libraryVersionsInfo; }
            set {

                SetProperty(ref libraryVersionsInfo, value);                 
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
                        
            LibraryVersionsInfo = sb.ToString();
        }
    }
}
