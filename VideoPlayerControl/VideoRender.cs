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

namespace VideoPlayerControl
{

    class VideoRender : IDisposable
    {

        public enum RenderMode
        {
            NORMAL,
            CLEAR_SCREEN,
            PAUSED
        };

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
            presentParams[0].PresentationInterval = D3D.PresentInterval.One;

            //flip frames on vsync
            presentParams[0].SwapEffect = D3D.SwapEffect.Discard;

            //Set Windowed vs. Full-screen
            presentParams[0].Windowed = windowed;

            //We only need to set the Width/Height in full-screen mode
            if (!windowed)
            {
                presentParams[0].BackBufferHeight = 1080;
                presentParams[0].BackBufferWidth = 1920;

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
           
            if (direct3D == null) return;
           
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
            
        }

        
        void releaseResources()
        {
            Utils.removeAndDispose(ref backBuffer); 
            Utils.removeAndDispose(ref offscreen);                                 
            Utils.removeAndDispose(ref screenShot);
            
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
              
                }

                releaseResources();
                aquireResources();
         
                int sizeBytes = videoWidth * (videoHeight + videoHeight / 2);                           
                offscreenBuffer = new Byte[sizeBytes];

                //log.Info("Direct3D Initialized");

            }
            catch (SharpDX.SharpDXException e)
            {
                throw new VideoPlayerException("Direct3D Initialization error: " + e.Message, e);              
            }

        }                                           

        public void createScreenShot(String screenShotName, int positionSeconds, String videoLocation, int offsetSeconds)
        {
            lock (renderLock)
            {
                if (device == null) return;

                int width = offscreen.Description.Width;
                int height = offscreen.Description.Height;

                SharpDX.Rectangle videoRect = new SharpDX.Rectangle(0, 0, width, height);

                device.StretchRectangle(offscreen, videoRect,
                    screenShot, videoRect, D3D.TextureFilter.Linear);

                SharpDX.DataRectangle stream = screenShot.LockRectangle(videoRect, D3D.LockFlags.ReadOnly);
                try
                {
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    BitmapMetadata metaData = new BitmapMetadata("jpg");

                    UriBuilder uri = new UriBuilder(new Uri(videoLocation).AbsoluteUri);
                 
                    TimeSpan time = new TimeSpan(0, 0, Math.Max(positionSeconds + offsetSeconds, 0));
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
                        width,
                        height,
                        96,
                        96,
                        System.Windows.Media.PixelFormats.Bgra32,
                        null,
                        stream.DataPointer,
                        height * stream.Pitch,
                        stream.Pitch
                    );
                   
                    float scale = ImageUtils.resizeRectangle(width, height, 240, 180);

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
            Rectangle videoRect = new Rectangle(0, 0, videoWidth, videoHeight);

            Rectangle scaledVideo = Utils.stretchRectangle(videoRect, screenRect);

            Rectangle scaledCenteredVideo = Utils.centerRectangle(screenRect, scaledVideo);

            SharpDX.Rectangle scaledCenteredVideoDx = new SharpDX.Rectangle(scaledCenteredVideo.Left,
                           scaledCenteredVideo.Top, scaledCenteredVideo.Right, scaledCenteredVideo.Bottom);

            return (scaledCenteredVideoDx);
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
                                          
                        SharpDX.Rectangle videoDestRect = getVideoDestRect(backBuffer);

                        device.StretchRectangle(offscreen, videoSourceRect,
                            backBuffer, videoDestRect, D3D.TextureFilter.Linear);

                        device.EndScene();
                        device.Present();                

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

        

    }
}
