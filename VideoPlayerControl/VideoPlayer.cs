using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;

//http://stackoverflow.com/questions/6036631/why-does-the-wpf-designer-fail-to-load-libraries-that-call-into-unmanaged-dlls

namespace VideoPlayerControl
{
    public partial class VideoPlayer: UserControl
    {
        public VideoPlayer()
        {          
            InitializeComponent();
         
            viewModel = new VideoPlayerViewModel(this, VideoLib.VideoPlayer.OutputPixelFormat.YUV420P);
                   
            ContextMenu mnuContextMenu = new ContextMenu();

            this.ContextMenu = mnuContextMenu;

            MenuItem toggleDebugInfoMenuItem = new MenuItem();
            toggleDebugInfoMenuItem.Text = "&Display Info";
            
            mnuContextMenu.MenuItems.Add(toggleDebugInfoMenuItem);
           
            toggleDebugInfoMenuItem.Click += toggleDebugInfoMenuItem_Click;

            // lag options           
            mnuContextMenu.MenuItems.Add("-");

            MenuItem toggleVideoLagMenuItem = new MenuItem();
            toggleVideoLagMenuItem.Text = "&Video Lag";
            toggleVideoLagMenuItem.Click += toggleVideoLagMenuItem_Click;
            mnuContextMenu.MenuItems.Add(toggleVideoLagMenuItem);

            MenuItem toggleAudioLagMenuItem = new MenuItem();
            toggleAudioLagMenuItem.Text = "&Audio Lag";
            toggleAudioLagMenuItem.Click += toggleAudioLagMenuItem_Click;
            mnuContextMenu.MenuItems.Add(toggleAudioLagMenuItem);

            MenuItem toggleVideoAudioLagMenuItem = new MenuItem();
            toggleVideoAudioLagMenuItem.Text = "&Audio and Video Lag";
            toggleVideoAudioLagMenuItem.Click += toggleVideoAudioLagMenuItem_Click;
            mnuContextMenu.MenuItems.Add(toggleVideoAudioLagMenuItem);
           
        }

        void toggleVideoLagMenuItem_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            menuItem.Checked = !menuItem.Checked;
            viewModel.simulateLag(0, menuItem.Checked);
        }

        void toggleAudioLagMenuItem_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            menuItem.Checked = !menuItem.Checked;
            viewModel.simulateLag(1, menuItem.Checked);
        }

        void toggleVideoAudioLagMenuItem_Click(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            menuItem.Checked = !menuItem.Checked;
            viewModel.simulateLag(0, menuItem.Checked);
            viewModel.simulateLag(1, menuItem.Checked);
        }

        void toggleDebugInfoMenuItem_Click(object sender, EventArgs e)
        {
            MenuItem toggleDebugInfoMenuItem = sender as MenuItem;

            viewModel.DisplayOverlayText = !viewModel.DisplayOverlayText;
            toggleDebugInfoMenuItem.Checked = viewModel.DisplayOverlayText;
        }

        VideoPlayerViewModel viewModel;

        public VideoPlayerViewModel ViewModel
        {
            get { return viewModel; }
            private set { viewModel = value; }
        }

        private void videoPlayer_Resize(object sender, EventArgs e)
        {         
            ViewModel.resize();
        }


    }
}
