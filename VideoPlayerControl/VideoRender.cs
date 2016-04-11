using System;
using System.Windows;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoLib;
using D3D = SharpDX.Direct3D9;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using System.IO;
using SharpDX.Direct3D9;
using MediaViewer.Infrastructure.Utils;
using MediaViewer.Infrastructure;
using SubtitlesParser.Classes;

namespace VideoPlayerControl
{

    public enum RenderMode
    {
        NORMAL,
        CLEAR_SCREEN,
        PAUSED
    };

    public enum AspectRatio
    {
        DEFAULT,
        RATIO_1_1,        
        RATIO_4_3,
        RATIO_16_9,
        RATIO_16_10,
        RATIO_221_1,
        RATIO_235_1,
        RATIO_239_1,
        RATIO_5_4
    };


    class VideoRender : IDisposable
    {
        public RenderMode RenderMode { get; protected set; }
        public AspectRatio AspectRatio { get; set; }
       
        public VideoRender(System.Windows.Forms.Control owner)
        {
            direct3D = null;
            device = null;
            this.owner = owner;       
            windowed = true;
            renderLock = new object();

            backBuffer = null;
            offscreen = null;
            screenShot = null;

            InfoText = null;
            DisplayInfoText = false;

            SubtitleItem = null;
            DisplaySubtitles = true;

            RenderMode = VideoPlayerControl.RenderMode.PAUSED;
            AspectRatio = VideoPlayerControl.AspectRatio.DEFAULT;
        }

        public void Dispose()
        {
            releaseResources();

            if (device != null)
            {
                device.Dispose();
                device = null;
            }
            if (direct3D != null)
            {
                direct3D.Dispose();
                direct3D = null;
            }                 
        }

        D3D.Direct3D direct3D;
        D3D.Device device;
        D3D.Surface backBuffer;
        D3D.Surface offscreen;
        D3D.Surface screenShot;
        byte[] offscreenBuffer;

        Object renderLock;

        int videoWidth;
        int videoHeight;

        bool windowed;

        public bool Windowed
        {
            get { return windowed; }
            protected set { windowed = value; }
        }

        public String InfoText { get; set; }
        public bool DisplayInfoText { get; set; }
        D3D.Font infoFont;

        public SubtitleItem SubtitleItem { get; set; }
        public bool DisplaySubtitles { get; set; }
        D3D.Font subtitleFont;

        int subtitleShadowOffset;
        int subtitleBottomMargin;

        SharpDX.Rectangle videoDestRect;

        System.Windows.Forms.Control owner;

        static D3D.Format makeFourCC(int ch0, int ch1, int ch2, int ch3)
        {
            int value = (int)(char)(ch0) | ((int)(char)(ch1) << 8) | ((int)(char)(ch2) << 16) | ((int)(char)(ch3) << 24);
            return ((D3D.Format)value);
        }

        D3D.PresentParameters[] createPresentParams(bool windowed, System.Windows.Forms.Control owner)
        {

            D3D.PresentParameters[] presentParams = new D3D.PresentParameters[1];

            //No Z (Depth) buffer or Stencil buffer
            presentParams[0].EnableAutoDepthStencil = false;
                 
            //multiple backbuffers for a flipchain
            presentParams[0].BackBufferCount = 3;

            //Set our Window as the Device Window
            presentParams[0].DeviceWindowHandle = owner.Handle;

            //wait for VSync
            presentParams[0].PresentationInterval = D3D.PresentInterval.Immediate;

            //flip frames on vsync
            presentParams[0].SwapEffect = D3D.SwapEffect.Discard;

            //Set Windowed vs. Full-screen
            presentParams[0].Windowed = windowed;

            //We only need to set the Width/Height in full-screen mode
            if (!windowed)
            {
                presentParams[0].BackBufferHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
                presentParams[0].BackBufferWidth = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Width;

                D3D.Format format = D3D.Format.X8R8G8B8;

                //Choose a compatible 16-bit mode.
                presentParams[0].BackBufferFormat = format;
               
            }
            else
            {
                presentParams[0].BackBufferHeight = 0;
                presentParams[0].BackBufferWidth = 0;
                presentParams[0].BackBufferFormat = D3D.Format.Unknown;
            }

            return (presentParams);
        }


        void reset()
        {
            if (offscreen == null) return;
           
			SharpDX.DataRectangle stream = offscreen.LockRectangle(LockFlags.ReadOnly);
			
            VideoFrame.copySurfaceToBuffer(stream.DataPointer, 
                offscreen.Description.Width, offscreen.Description.Height, stream.Pitch, offscreenBuffer);

            offscreen.UnlockRectangle();

            releaseResources();

            D3D.PresentParameters[] presentParams = createPresentParams(windowed, owner);

            device.Reset(presentParams);
                          
            aquireResources();
           
            stream = offscreen.LockRectangle(LockFlags.None);

            VideoFrame.copyBufferToSurface(offscreenBuffer, stream.DataPointer,
                offscreen.Description.Width, offscreen.Description.Height, stream.Pitch);

            offscreen.UnlockRectangle();              
            
        }


        void resetDevice()
        {

            if (owner.InvokeRequired)
            {
                owner.Invoke(new Action(reset));               
            }
            else
            {
                reset();
            }
        
        }

        void aquireResources()
        {

            if (videoWidth == 0 || videoHeight == 0)
            {
                throw new VideoPlayerException("Cannot instantiate D3D surface with a width or height of 0 pixels");
            }

            D3D.Format pixelFormat = makeFourCC('Y', 'V', '1', '2');

            offscreen = D3D.Surface.CreateOffscreenPlain(device,
                videoWidth,
                videoHeight,
                pixelFormat,
                D3D.Pool.Default);

            screenShot = D3D.Surface.CreateOffscreenPlain(device,
                videoWidth,
                videoHeight,
                D3D.Format.A8R8G8B8,
                D3D.Pool.Default);

            backBuffer = device.GetBackBuffer(0, 0);

            FontDescription fontDescription = new FontDescription();
            fontDescription.FaceName = "TimesNewRoman";
            fontDescription.Height = 15;         

            infoFont = new D3D.Font(device, fontDescription);

            fontDescription = new FontDescription();
            fontDescription.FaceName = "Arial";

            videoDestRect = getVideoDestRect(backBuffer);
       
            fontDescription.Height = videoDestRect.Height / 14;
             
            fontDescription.Quality = FontQuality.Antialiased;

            subtitleShadowOffset = fontDescription.Height / 18;
            subtitleBottomMargin = videoDestRect.Height / 12;

            subtitleFont = new D3D.Font(device, fontDescription);
        }

        
        public void releaseResources()
        {
            Utils.removeAndDispose(ref backBuffer); 
            Utils.removeAndDispose(ref offscreen);                                 
            Utils.removeAndDispose(ref screenShot);
            Utils.removeAndDispose(ref infoFont);
            Utils.removeAndDispose(ref subtitleFont);            
        }

        public void setWindowed()
        {           
            lock (renderLock)
            {               
                if (device != null)
                {
                    windowed = true;

                    resetDevice();
                }
            }
        }

        public void setFullScreen()
        {
            lock (renderLock)
            {
                if (device != null)
                {
                    windowed = false;

                    resetDevice();
                }
            }
        }

        public void resize()
        {
            lock (renderLock)
            {
                resetDevice();
            }
        }


        public void initialize(int videoWidth, int videoHeight)
        {
            try
            {
                this.videoHeight = videoHeight;
                this.videoWidth = videoWidth;

                if (direct3D == null)
                {

                    SharpDX.Result resultCode;

                    direct3D = new D3D.Direct3D();

                    if (direct3D.CheckDeviceFormatConversion(
                        0,
                        D3D.DeviceType.Hardware,
                        makeFourCC('Y', 'V', '1', '2'),
                        D3D.Format.X8R8G8B8,
                        out resultCode) == false)
                    {
                        throw new SharpDX.SharpDXException("Video Hardware does not support YV12 format conversion");
                    }

                    D3D.PresentParameters[] presentParams = createPresentParams(windowed, owner);

                    device = new D3D.Device(direct3D,
                        0,
                        D3D.DeviceType.Hardware,
                        owner.Handle,
                        D3D.CreateFlags.SoftwareVertexProcessing,
                        presentParams);

                    releaseResources();
                    aquireResources();
                }
                else
                {
                    releaseResources();

                    D3D.PresentParameters[] presentParams = createPresentParams(windowed, owner);

                    device.Reset(presentParams);

                    aquireResources();

                }

                int sizeBytes = videoWidth * (videoHeight + videoHeight / 2);                           
                offscreenBuffer = new Byte[sizeBytes];

                //log.Info("Direct3D Initialized");

            }
            catch (SharpDX.SharpDXException e)
            {
                throw new VideoPlayerException("Direct3D Initialization error: " + e.Message, e);              
            }

        }                                           

        public void createScreenShot(String screenShotName, double positionSeconds, String videoLocation, int offsetSeconds)
        {
            lock (renderLock)
            {
                if (device == null) return;

                int width = offscreen.Description.Width;
                int height = offscreen.Description.Height;

                Rectangle sourceSize = new Rectangle(0, 0, width, height);
                Rectangle destSize = calcOutputAspectRatio(sourceSize);

                if (screenShot.Description.Width != destSize.Width ||
                    screenShot.Description.Height != destSize.Height)
                {
                    Utils.removeAndDispose(ref screenShot); 

                    screenShot = D3D.Surface.CreateRenderTarget(device,
                        destSize.Width,
                        destSize.Height,
                        D3D.Format.A8R8G8B8,
                        MultisampleType.None,
                        0,
                        true);
                }

                SharpDX.Rectangle sourceSizeDX = new SharpDX.Rectangle(0, 0, sourceSize.Width, sourceSize.Height);
                SharpDX.Rectangle destSizeDX = new SharpDX.Rectangle(0, 0, destSize.Width, destSize.Height);

                device.StretchRectangle(offscreen, sourceSizeDX,
                    screenShot, destSizeDX, D3D.TextureFilter.Linear);

                SharpDX.DataRectangle stream = screenShot.LockRectangle(destSizeDX, D3D.LockFlags.ReadOnly);
                try
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    BitmapMetadata metaData = new BitmapMetadata("jpg");

                    UriBuilder uri = new UriBuilder(new Uri(videoLocation).AbsoluteUri);
                 
                    int seconds = (int)Math.Floor(Math.Max(positionSeconds + offsetSeconds, 0));

                    TimeSpan time = new TimeSpan(0, 0, seconds);
                    String timeString = "";

                    if (time.Days > 0)
                    {
                        timeString += time.Days + "d";
                    }

                    if(time.Hours > 0) {
                        timeString += time.Hours + "h";
                    }

                    if(time.Minutes > 0) {
                        timeString += time.Minutes + "m";
                    }

                    timeString += time.Seconds + "s";

                    uri.Query = "t=" + timeString;

                    metaData.ApplicationName = "MediaViewer v1.0";             
                    metaData.Title = uri.ToString();
                    metaData.DateTaken = DateTime.Now.ToString("R");

                    BitmapSource image = System.Windows.Media.Imaging.BitmapSource.Create(
                        destSize.Width,
                        destSize.Height,
                        96,
                        96,
                        System.Windows.Media.PixelFormats.Bgra32,
                        null,
                        stream.DataPointer,
                        height * stream.Pitch,
                        stream.Pitch
                    );

                    float scale = ImageUtils.resizeRectangle(destSize.Width, destSize.Height, Constants.MAX_THUMBNAIL_WIDTH, Constants.MAX_THUMBNAIL_HEIGHT);

                    TransformedBitmap thumbnail = new TransformedBitmap(image, new System.Windows.Media.ScaleTransform(scale,scale));
                   
                    encoder.Frames.Add(BitmapFrame.Create(image, thumbnail, metaData, null));
                                                                        
                    FileStream outputFile = new FileStream(screenShotName, FileMode.Create);
                    //encoder.QualityLevel = asyncState.JpegQuality;
                    encoder.Save(outputFile);

                    outputFile.Close();
                                  
                    System.Media.SystemSounds.Exclamation.Play();
                }
                catch (Exception e)
                {                  
                    throw new VideoPlayerException("Error creating screenshot: " + e.Message, e);
                }
                finally
                {
                    screenShot.UnlockRectangle();
                }
            }
        }

        SharpDX.Rectangle getVideoDestRect(D3D.Surface backBuffer)
        {
            Rectangle screenRect = new Rectangle(0, 0, backBuffer.Description.Width, backBuffer.Description.Height);
            Rectangle videoRect = calcOutputAspectRatio(new Rectangle(0, 0, videoWidth, videoHeight));

            Rectangle scaledVideo = Utils.stretchRectangle(videoRect, screenRect);

            Rectangle scaledCenteredVideo = Utils.centerRectangle(screenRect, scaledVideo);

            SharpDX.Rectangle scaledCenteredVideoDx = new SharpDX.Rectangle(scaledCenteredVideo.X,
                           scaledCenteredVideo.Y, scaledCenteredVideo.Width, scaledCenteredVideo.Height);

            return (scaledCenteredVideoDx);
        }

        Rectangle calcOutputAspectRatio(Rectangle inputSize)
        {
            Rectangle outputSize = inputSize;

            switch (AspectRatio)
            {
                case AspectRatio.DEFAULT:
                    break;            
                case AspectRatio.RATIO_1_1:
                    outputSize = new Rectangle(0,0,inputSize.Height, inputSize.Height);
                    break;
                case AspectRatio.RATIO_16_9:             
                    outputSize = new Rectangle(0, 0, (int)(inputSize.Height / 9.0 * 16), inputSize.Height);
                    break;
                case AspectRatio.RATIO_4_3:
                    outputSize = new Rectangle(0, 0, (int)(inputSize.Height / 3 * 4), inputSize.Height);
                    break;
                case AspectRatio.RATIO_16_10:
                    outputSize = new Rectangle(0, 0, (int)(inputSize.Height / 10 * 16), inputSize.Height);
                    break;
                case AspectRatio.RATIO_5_4:
                    outputSize = new Rectangle(0, 0, (int)(inputSize.Height / 4 * 5), inputSize.Height);
                    break;
                case AspectRatio.RATIO_221_1:
                    outputSize = new Rectangle(0, 0, (int)(inputSize.Height * 2.21), inputSize.Height);
                    break;
                case AspectRatio.RATIO_235_1:
                    outputSize = new Rectangle(0, 0, (int)(inputSize.Height * 2.35), inputSize.Height);
                    break;
                case AspectRatio.RATIO_239_1:
                    outputSize = new Rectangle(0, 0, (int)(inputSize.Height * 2.39), inputSize.Height);
                    break;
                default:
                    break;
            }

            return(outputSize);
        }

        public void display(VideoFrame videoFrame, Color backColor, RenderMode mode)
        {
            
            lock (renderLock)
            {
                if (device == null) return;          

                SharpDX.Result deviceStatus = device.TestCooperativeLevel();
          
                if (deviceStatus.Success == true)
                {
                    try
                    {
                        SharpDX.ColorBGRA backColorDX = new SharpDX.ColorBGRA(backColor.R,
                            backColor.G, backColor.B, backColor.A);

                        device.Clear(D3D.ClearFlags.Target, backColorDX, 1.0f, 0);

                        if (mode == RenderMode.CLEAR_SCREEN)
                        {
                            device.Present();
                            return;
                        }           

                        device.BeginScene();
                 
                        SharpDX.Rectangle videoSourceRect = new SharpDX.Rectangle();

                        if (mode == RenderMode.NORMAL)
                        {

                            videoSourceRect = new SharpDX.Rectangle(0, 0, videoFrame.Width, videoFrame.Height);

                            SharpDX.DataRectangle stream = offscreen.LockRectangle(LockFlags.None);

                            videoFrame.copyFrameDataToSurface(stream.DataPointer, stream.Pitch);

                            offscreen.UnlockRectangle();

                        }
                        else if (mode == RenderMode.PAUSED)
                        {
                            videoSourceRect = new SharpDX.Rectangle(0, 0, offscreen.Description.Width, offscreen.Description.Height);
                        }
                                          
                        videoDestRect = getVideoDestRect(backBuffer);

                        device.StretchRectangle(offscreen, videoSourceRect,
                            backBuffer, videoDestRect, D3D.TextureFilter.Linear);

                        drawText();

                        device.EndScene();
                        device.Present();

                        RenderMode = mode;

                    }
                    catch (SharpDX.SharpDXException)
                    {

                        //log.Info("lost direct3d device", e);
                        deviceStatus = device.TestCooperativeLevel();
                    }
                }

                if (deviceStatus.Code == D3D.ResultCode.DeviceLost.Result)
                {

                    //Can't Reset yet, wait for a bit

                }
                else if (deviceStatus.Code == D3D.ResultCode.DeviceNotReset.Result)
                {
                    
                    resetDevice();
                }
            }
        }

        private void drawText()
        {
            
            if (DisplayInfoText && !String.IsNullOrEmpty(InfoText))
            {
                //SharpDX.Rectangle size = infoFont.MeasureText(null, InfoText, FontDrawFlags.Left);
                //Rectangle.
                
                    //AABBGGRR
                infoFont.DrawText(null, InfoText, 5, 0, SharpDX.ColorBGRA.FromRgba(0xFFFFFFFF));
            }

            if (DisplaySubtitles && SubtitleItem != null && SubtitleItem.Lines.Count > 0)
            {                
                String lines = SubtitleItem.Lines[0];
                for(int i = 1; i < SubtitleItem.Lines.Count; i++)
                {
                    lines += "\n" + SubtitleItem.Lines[i];
                }
               
                SharpDX.Rectangle fontRect = subtitleFont.MeasureText(null, lines, FontDrawFlags.Center);
                
                fontRect.X = backBuffer.Description.Width / 2 + fontRect.X;
                fontRect.Y = videoDestRect.BottomLeft.Y - fontRect.Height - subtitleBottomMargin;
            
                SharpDX.Rectangle shadowRect = new SharpDX.Rectangle(fontRect.X + subtitleShadowOffset, fontRect.Y + subtitleShadowOffset, fontRect.Width, fontRect.Height);

                subtitleFont.DrawText(null, lines, shadowRect, FontDrawFlags.Center, SharpDX.ColorBGRA.FromRgba(0xCC000000));
                subtitleFont.DrawText(null, lines, fontRect, FontDrawFlags.Center, SharpDX.ColorBGRA.FromRgba(0xFFF9FBFB));                
            }
            
        }        

        

    }
}
