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


namespace VideoPlayerControl
{

    class VideoRender
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
            saveOffscreen = null;
            windowed = true;
            renderLock = new object();
        }

        ~VideoRender()
        {
            releaseResources();

            removeAndDispose(ref saveOffscreen);
            removeAndDispose(ref device);
            removeAndDispose(ref direct3D);        
        }

        D3D.Direct3D direct3D;
        D3D.Device device;
        D3D.Surface offscreen;
        D3D.Surface screenShot;
        VideoFrame saveOffscreen;

        Object renderLock;

        int videoWidth;
        int videoHeight;

        bool windowed;
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
                presentParams[0].BackBufferHeight = owner.Height;
                presentParams[0].BackBufferWidth = owner.Width;

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
            lock (renderLock)
            {
                if (direct3D == null) return;

                releaseResources();
          
                //device.Reset(createPresentParams(windowed, owner));

                removeAndDispose(ref device);

                D3D.PresentParameters[] presentParams = createPresentParams(windowed, owner);

                device = new D3D.Device(direct3D,
                    0,
                    D3D.DeviceType.Hardware,
                    owner.Handle,
                    D3D.CreateFlags.SoftwareVertexProcessing,
                    presentParams);
             
                aquireResources();
                D3D.Surface backBuffer = device.GetBackBuffer(0, 0);
                                  
            }
        }


        public void resetDevice()
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

            if (videoWidth != 0 && videoHeight != 0)
            {

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
            }
        }

        
        void releaseResources()
        {

            if (offscreen != null)
            {
                removeAndDispose(ref offscreen);                       
                offscreen = null;
            }

            if (screenShot != null)
            {
                removeAndDispose(ref screenShot);   
                screenShot = null;
            }
        }

        public void setWindowed()
        {
            windowed = true;

            if (device != null)
            {

                device.Reset(createPresentParams(windowed, owner));
            }
        }


        public void setFullScreen()
        {
            windowed = false;

            if (device != null)
            {
                try
                {
                    device.Reset(createPresentParams(windowed, owner));
                }
                catch (Exception e)
                {

                    //log.Error("Error setting fullscreen", e);
                }
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
        
            
                //log.Info("Direct3D Initialized");

            }
            catch (SharpDX.SharpDXException e)
            {

                //log.Error("Direct3D Initialization error", e);
                MessageBox.Show(e.Message, "Direct3D Initialization error");

            }

        }

        public void createScreenShot(String fileName)
        {

            if (device == null) return;

            try
            {

                int width = offscreen.Description.Width;
                int height = offscreen.Description.Height;

                SharpDX.Rectangle videoRect = new SharpDX.Rectangle(0, 0, width, height);

                device.StretchRectangle(offscreen, videoRect,
                    screenShot, videoRect, D3D.TextureFilter.Linear);

                SharpDX.DataRectangle stream = screenShot.LockRectangle(videoRect, D3D.LockFlags.ReadOnly);

                Bitmap image = new Bitmap(width, height, stream.Pitch,
                    System.Drawing.Imaging.PixelFormat.Format32bppArgb, stream.DataPointer);

                /*
                            String path = Util.getPathWithoutFileName(fileName);
                            fileName = System.IO.Path.GetFileNameWithoutExtension(fileName);
                            fileName += "." + Settings.getVar(Settings.VarName.VIDEO_SCREENSHOT_FILE_TYPE);

                            fileName = FileUtils.getUniqueFileName(path + "\\" + fileName);

                            image.Save(fileName);*/
                image.Save("dummy.jpg");

                screenShot.UnlockRectangle();

            }
            catch (Exception e)
            {

                //log.Error("Screenshot failed", e);
                MessageBox.Show("Screenshot failed: " + e.Message, "Error");
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

                            videoFrame.copyFrameDataToSurface(offscreen);

                        }
                        else if (mode == RenderMode.PAUSED)
                        {

                            videoSourceRect = new SharpDX.Rectangle(0, 0, offscreen.Description.Width, offscreen.Description.Height);

                        }

                        D3D.Surface backBuffer = device.GetBackBuffer(0, 0);

                        SharpDX.Rectangle videoDestRect = getVideoDestRect(backBuffer);

                        device.StretchRectangle(offscreen, videoSourceRect,
                            backBuffer, videoDestRect, D3D.TextureFilter.Linear);

                        device.EndScene();
                        device.Present();

                    }
                    catch (SharpDX.SharpDXException e)
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

        void removeAndDispose<TypeName>(ref TypeName resource) where TypeName : class
        {
            if (resource == null)
                return;

            IDisposable disposer = resource as IDisposable;
            if (disposer != null)
            {
                try
                {
                    disposer.Dispose();
                }
                catch
                {
                }
            }

            resource = null;
        }
    }
}
