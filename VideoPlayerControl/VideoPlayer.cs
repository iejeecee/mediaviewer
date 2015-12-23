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
using SubtitlesParser.Classes;
using System.IO;
using MediaViewer.Infrastructure.Utils;
using System.Reflection;

//http://stackoverflow.com/questions/6036631/why-does-the-wpf-designer-fail-to-load-libraries-that-call-into-unmanaged-dlls

namespace VideoPlayerControl
{
    public partial class VideoPlayer: UserControl
    {
        static Image playIcon;
        static Image pauseIcon;
        static Image closeIcon;

        static VideoPlayer()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
           
            Stream pauseStream = assembly.GetManifestResourceStream("VideoPlayerControl.Resources.ButtonPause.ico");
            pauseIcon = new Bitmap(pauseStream);

            Stream playStream = assembly.GetManifestResourceStream("VideoPlayerControl.Resources.ButtonPlay.ico");
            playIcon = new Bitmap(playStream);

            Stream closeStream = assembly.GetManifestResourceStream("VideoPlayerControl.Resources.ButtonClose.ico");
            closeIcon = new Bitmap(closeStream);
        }

        public VideoPlayer()
        {          
            InitializeComponent();
         
            viewModel = new VideoPlayerViewModel(this, VideoLib.VideoPlayer.OutputPixelFormat.YUV420P);
                   
            ContextMenuStrip contextMenu = new ContextMenuStrip();
        
            this.ContextMenuStrip = contextMenu;
            contextMenu.Opening += contextMenu_Popup;

            ToolStripMenuItem playPauseMenuItem = new ToolStripMenuItem();
            playPauseMenuItem.Name = "playpause";           
            contextMenu.Items.Add(playPauseMenuItem);
            playPauseMenuItem.Click += playPauseMenuItem_Click;

            ToolStripMenuItem stopMenuItem = new ToolStripMenuItem();
            stopMenuItem.Name = "close";
            stopMenuItem.Text = "&Stop";
            stopMenuItem.Image = closeIcon;
            contextMenu.Items.Add(stopMenuItem);
            stopMenuItem.Click += stopMenuItem_Click;

            contextMenu.Items.Add("-");

            createAudioContextMenu(contextMenu);
            createVideoContextMenu(contextMenu);
            createSubtitleContextMenu(contextMenu);
                                    
#if DEBUG
            contextMenu.Items.Add("-");
            createDebugContextMenu(contextMenu);
#endif           
        }

        private async void stopMenuItem_Click(object sender, EventArgs e)
        {
            await ViewModel.close();
        }

        private void playPauseMenuItem_Click(object sender, EventArgs e)
        {        
            if (ViewModel.VideoState == VideoState.PLAYING)
            {
                ViewModel.pause();
            }
            else
            {
                ViewModel.play();
            }
        }

        void contextMenu_Popup(object sender, EventArgs e)
        {
            ContextMenuStrip contextMenu = (ContextMenuStrip)sender;

            ToolStripMenuItem playPauseMenuItem = (ToolStripMenuItem)contextMenu.Items.Find("playpause", false)[0];

            if (ViewModel.VideoState == VideoState.PLAYING)
            {
                playPauseMenuItem.Text = "&Pause";
                playPauseMenuItem.Image = pauseIcon;
            }
            else
            {
                playPauseMenuItem.Text = "&Play";
                playPauseMenuItem.Image = playIcon;
            }

            playPauseMenuItem.Enabled = ViewModel.VideoState != VideoState.CLOSED;

            ToolStripMenuItem closeMenuItem = (ToolStripMenuItem)contextMenu.Items.Find("close", false)[0];
            closeMenuItem.Enabled = ViewModel.VideoState != VideoState.CLOSED;

            ToolStripMenuItem subtitle = (ToolStripMenuItem)contextMenu.Items.Find("subtitle", false)[0];

            bool hasVideo = !String.IsNullOrEmpty(ViewModel.VideoLocation);

            subtitle.Enabled = hasVideo;           
                        
        }
        
        private void createAudioContextMenu(System.Windows.Forms.ContextMenuStrip contextMenu)
        {
            ToolStripMenuItem audioMenuItem = new ToolStripMenuItem();
            audioMenuItem.Name = "audio";
            audioMenuItem.Text = "&Audio";
            contextMenu.Items.Add(audioMenuItem);
            audioMenuItem.DropDownOpening += audioMenuItem_DropDownOpening;
                       
            ToolStripMenuItem increaseVolumeMenuItem = new ToolStripMenuItem();
            increaseVolumeMenuItem.Text = "&Increase Volume";
            increaseVolumeMenuItem.Click += increaseVolumeMenuItem_Click;
            audioMenuItem.DropDownItems.Add(increaseVolumeMenuItem);

            ToolStripMenuItem decreaseVolumeMenuItem = new ToolStripMenuItem();
            decreaseVolumeMenuItem.Text = "&Decrease Volume";
            decreaseVolumeMenuItem.Click += decreaseVolumeMenuItem_Click;
            audioMenuItem.DropDownItems.Add(decreaseVolumeMenuItem);

            ToolStripMenuItem toggleMuteMenuItem = new ToolStripMenuItem();
            toggleMuteMenuItem.Name = "mute";
            toggleMuteMenuItem.Text = "&Mute";
            toggleMuteMenuItem.Click += toggleMuteMenuItem_Click;
            audioMenuItem.DropDownItems.Add(toggleMuteMenuItem);
            
        }

        private void decreaseVolumeMenuItem_Click(object sender, EventArgs e)
        {
            double step = (ViewModel.MinVolume - ViewModel.MaxVolume) / 10.0;
            ViewModel.Volume += step;
        }

        private void increaseVolumeMenuItem_Click(object sender, EventArgs e)
        {
            double step = (ViewModel.MinVolume - ViewModel.MaxVolume) / 10.0;
            ViewModel.Volume -= step;
        }

        private void toggleMuteMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toggleMuteMenuItem = sender as ToolStripMenuItem;
            ViewModel.IsMuted = !toggleMuteMenuItem.Checked;
        }

        void audioMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem audioMenuItem = sender as ToolStripMenuItem;
            ToolStripMenuItem toggleMuteMenuItem = audioMenuItem.DropDownItems.Find("mute", false)[0] as ToolStripMenuItem;

            toggleMuteMenuItem.Checked = viewModel.IsMuted;
        }

        void createVideoContextMenu(ContextMenuStrip contextMenu)
        {
            ToolStripMenuItem videoMenuItem = new ToolStripMenuItem();
            videoMenuItem.Name = "video";
            videoMenuItem.Text = "&Video";
            videoMenuItem.DropDownOpening += videoMenuItem_DropDownOpening;
            contextMenu.Items.Add(videoMenuItem);

            ToolStripMenuItem toggleShowInfoMenuItem = new ToolStripMenuItem();
            toggleShowInfoMenuItem.Text = "&Display Info";
            videoMenuItem.DropDownItems.Add(toggleShowInfoMenuItem);
            toggleShowInfoMenuItem.Checked = viewModel.DisplayInfoText;
            toggleShowInfoMenuItem.Click += toggleDebugInfoMenuItem_Click;

            ToolStripMenuItem toggleFullScreenMenuItem = new ToolStripMenuItem();
            toggleFullScreenMenuItem.Name = "toggleFullscreen";
            toggleFullScreenMenuItem.Text = "&Toggle Fullscreen";
            //videoMenuItem.DropDownItems.Add(toggleFullScreenMenuItem);
            toggleFullScreenMenuItem.Click += toggleFullScreenMenuItem_Click;
       
            createAspectRatioContextMenu(videoMenuItem);
        }

        private void toggleFullScreenMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toggleFullScreenMenuItem = sender as ToolStripMenuItem;
            ViewModel.IsFullScreen = !toggleFullScreenMenuItem.Checked;
        }

        void videoMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            ToolStripMenuItem videoMenuItem = sender as ToolStripMenuItem;
            //ToolStripMenuItem toggleFullScreenMenuItem = videoMenuItem.DropDownItems.Find("toggleFullscreen", false)[0] as ToolStripMenuItem;
            //toggleFullScreenMenuItem.Checked = viewModel.IsFullScreen;
        }

        void createAspectRatioContextMenu(ToolStripMenuItem contextMenu)
        {
            ToolStripMenuItem aspectRatioMenuItem = new ToolStripMenuItem();
            aspectRatioMenuItem.Name = "aspectRatio";
            aspectRatioMenuItem.Text = "&Aspect Ratio";
            contextMenu.DropDownItems.Add(aspectRatioMenuItem);

            ToolStripMenuItem aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.DEFAULT;
            aspectRatioItemMenuItem.Checked = true;
            aspectRatioItemMenuItem.Text = "&Default";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;

            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_1_1;
            aspectRatioItemMenuItem.Text = "&1:1";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;
            
            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_4_3;
            aspectRatioItemMenuItem.Text = "&4:3";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;

            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_16_9;
            aspectRatioItemMenuItem.Text = "&16:9";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;

            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_16_10;
            aspectRatioItemMenuItem.Text = "&16:10";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;

            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_221_1;
            aspectRatioItemMenuItem.Text = "&2.21:1";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;
            
            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_235_1;
            aspectRatioItemMenuItem.Text = "&2.35:1";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;

            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_239_1;
            aspectRatioItemMenuItem.Text = "&2.39:1";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;

            aspectRatioItemMenuItem = new ToolStripMenuItem();
            aspectRatioItemMenuItem.Tag = AspectRatio.RATIO_5_4;
            aspectRatioItemMenuItem.Text = "&5:4";
            aspectRatioMenuItem.DropDownItems.Add(aspectRatioItemMenuItem);
            aspectRatioItemMenuItem.Click += aspectRatioItemMenuItem_Click;
            
        }

        private void aspectRatioItemMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem aspectRatioItemMenuItem = sender as ToolStripMenuItem;
          
            foreach(ToolStripMenuItem child in aspectRatioItemMenuItem.GetCurrentParent().Items) {
              
                child.Checked = false;
            }

            aspectRatioItemMenuItem.Checked = true;
            ViewModel.AspectRatio = (AspectRatio)aspectRatioItemMenuItem.Tag;
        }

        void createSubtitleContextMenu(ContextMenuStrip menu)
        {
            ToolStripMenuItem subtitlesMenuItem = new ToolStripMenuItem();
            subtitlesMenuItem.Name = "subtitle";
            subtitlesMenuItem.Text = "&Subtitle";
            subtitlesMenuItem.DropDownOpening += subtitlesMenuItem_Popup;
            menu.Items.Add(subtitlesMenuItem);

            subtitlesMenuItem.Click += disableSubtitlesMenuItem_Click;

            ToolStripMenuItem addSubtitlesMenuItem = new ToolStripMenuItem();
            addSubtitlesMenuItem.Text = "&Add Subtitle File...";
            subtitlesMenuItem.DropDownItems.Add(addSubtitlesMenuItem);
            addSubtitlesMenuItem.Click += addSubtitlesMenuItem_Click;

            ToolStripMenuItem subtitleTrackMenuItem = new ToolStripMenuItem();
            subtitleTrackMenuItem.Name = "Track";
            subtitleTrackMenuItem.Text = "&Track";
            subtitleTrackMenuItem.DropDownOpening += subtitleTrackMenuItem_Popup;
            subtitleTrackMenuItem.DropDownItems.Add(new ToolStripMenuItem());
            subtitlesMenuItem.DropDownItems.Add(subtitleTrackMenuItem);

        }

        void subtitlesMenuItem_Popup(object sender, EventArgs e)
        {
            ToolStripMenuItem subtitleMenuItem = (ToolStripMenuItem)sender;
            ToolStripMenuItem trackMenuItem = (ToolStripMenuItem)subtitleMenuItem.DropDownItems.Find("track", false)[0];
            
            trackMenuItem.Enabled = ViewModel.Subtitles.NrTracks > 0;           
        }

        void subtitleTrackMenuItem_Popup(object sender, EventArgs e)
        {
            ToolStripMenuItem subtitleTrackMenuItem = (ToolStripMenuItem)sender;
       
            subtitleTrackMenuItem.DropDownItems.Clear();

            ToolStripMenuItem disableSubtitlesMenuItem = new ToolStripMenuItem();
            disableSubtitlesMenuItem.Text = "&Disable";
            disableSubtitlesMenuItem.Checked = !viewModel.DisplaySubtitles;
            subtitleTrackMenuItem.DropDownItems.Add(disableSubtitlesMenuItem);

            disableSubtitlesMenuItem.Click += disableSubtitlesMenuItem_Click;

            for (int i = 0; i < ViewModel.Subtitles.NrTracks; i++)
            {
                ToolStripMenuItem selectTrackMenuItem = new ToolStripMenuItem();
                selectTrackMenuItem.Text = "&Track " + i.ToString();
                selectTrackMenuItem.Tag = i;
                selectTrackMenuItem.Checked = viewModel.Subtitles.Track == i;
                selectTrackMenuItem.Click += selectTrackMenuItem_Click;

                subtitleTrackMenuItem.DropDownItems.Add(selectTrackMenuItem);
            }
            
        }

        void selectTrackMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem selectTrackMenuItem = sender as ToolStripMenuItem;
            for (int i = 1; i < selectTrackMenuItem.GetCurrentParent().Items.Count; i++)
            {
                ((ToolStripMenuItem)selectTrackMenuItem.GetCurrentParent().Items[i]).Checked = false;            
            }

            selectTrackMenuItem.Checked = true;

            ViewModel.Subtitles.Track = (int)selectTrackMenuItem.Tag;
        }

        private void disableSubtitlesMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem disableSubtitlesMenuItem = sender as ToolStripMenuItem;

            viewModel.DisplaySubtitles = !viewModel.DisplaySubtitles;
            disableSubtitlesMenuItem.Checked = !viewModel.DisplaySubtitles;
        }

        void createDebugContextMenu(ContextMenuStrip contextMenu)
        {
            
            ToolStripMenuItem debugMenuItem = new ToolStripMenuItem();    
            debugMenuItem.Text = "&Debug";
            contextMenu.Items.Add(debugMenuItem);

            ToolStripMenuItem toggleVideoLagMenuItem = new ToolStripMenuItem();
            toggleVideoLagMenuItem.Text = "&Video Lag";
            toggleVideoLagMenuItem.Click += toggleVideoLagMenuItem_Click;
            debugMenuItem.DropDownItems.Add(toggleVideoLagMenuItem);

            ToolStripMenuItem toggleAudioLagMenuItem = new ToolStripMenuItem();
            toggleAudioLagMenuItem.Text = "&Audio Lag";
            toggleAudioLagMenuItem.Click += toggleAudioLagMenuItem_Click;
            debugMenuItem.DropDownItems.Add(toggleAudioLagMenuItem);

            ToolStripMenuItem toggleVideoAudioLagMenuItem = new ToolStripMenuItem();
            toggleVideoAudioLagMenuItem.Text = "&Audio and Video Lag";
            toggleVideoAudioLagMenuItem.Click += toggleVideoAudioLagMenuItem_Click;
            debugMenuItem.DropDownItems.Add(toggleVideoAudioLagMenuItem);
        }

        void toggleVideoLagMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            menuItem.Checked = !menuItem.Checked;
            viewModel.simulateLag(0, menuItem.Checked);
        }

        void toggleAudioLagMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            menuItem.Checked = !menuItem.Checked;
            viewModel.simulateLag(1, menuItem.Checked);
        }

        void toggleVideoAudioLagMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem menuItem = sender as ToolStripMenuItem;
            menuItem.Checked = !menuItem.Checked;
            viewModel.simulateLag(0, menuItem.Checked);
            viewModel.simulateLag(1, menuItem.Checked);
        }

        void toggleDebugInfoMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem toggleDebugInfoMenuItem = sender as ToolStripMenuItem;

            viewModel.DisplayInfoText = !viewModel.DisplayInfoText;
            toggleDebugInfoMenuItem.Checked = viewModel.DisplayInfoText;
        }

        void addSubtitlesMenuItem_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(ViewModel.VideoLocation) || ImageUtils.isUrl(ViewModel.VideoLocation)) return;

            OpenFileDialog openFileDialog = new OpenFileDialog();
                       
            foreach (SubtitlesFormat format in SubtitlesFormat.SupportedSubtitlesFormats)
            {
                String ext = format.Extension;
                if (ext == null) continue;

                String filter = format.Name + "|" + "*" + ext.TrimStart(new char[] { '\\' });

                if (String.IsNullOrEmpty(openFileDialog.Filter))
                {
                    openFileDialog.Filter = filter;
                }
                else
                {
                    openFileDialog.Filter += "|" + filter;
                }
            }

            openFileDialog.FilterIndex = 1;

            openFileDialog.InitialDirectory = Path.GetDirectoryName(ViewModel.VideoLocation);

            DialogResult result = openFileDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                ViewModel.Subtitles.addSubtitleFile(openFileDialog.FileName);
                ViewModel.Subtitles.Track = ViewModel.Subtitles.NrTracks - 1;
            }
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
