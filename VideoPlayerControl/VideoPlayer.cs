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
