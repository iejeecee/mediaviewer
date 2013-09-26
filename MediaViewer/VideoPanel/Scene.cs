//http://www.codeproject.com/Articles/207642/Video-Shadering-with-Direct3D
//https://github.com/RobyDX/SharpDX_Demo/tree/master/SharpDXTutorial
// Copyright (c) 2010-2012 SharpDX - Alexandre Mutel
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// -----------------------------------------------------------------------------
// Original code from SlimMath project. http://code.google.com/p/slimmath/
// Greetings to SlimDX Group. Original code published with the following license:
// -----------------------------------------------------------------------------
/*
* Copyright (c) 2007-2011 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/
namespace MediaViewer.VideoPanel
{
    using System;
    using SharpDX;
    using SharpDX.D3DCompiler;
    using SharpDX.Direct3D10;
    using SharpDX.DXGI;
    using Buffer = SharpDX.Direct3D10.Buffer;
    using Device = SharpDX.Direct3D10.Device;
    using System.Windows.Media.Imaging;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;
    using MediaViewer.Timers;
using VideoLib;

    public class Scene : IScene
    {

        // no AV sync correction is done if below the AV sync threshold 
        const double AV_SYNC_THRESHOLD = 0.01;
        // no AV sync correction is done if too big error 
        const double AV_NOSYNC_THRESHOLD = 10.0;

        const double AUDIO_SAMPLE_CORRECTION_PERCENT_MAX = 10;

        // we use about AUDIO_DIFF_AVG_NB A-V differences to make the average 
        const int AUDIO_DIFF_AVG_NB = 5;//20;

        VideoLib.VideoPlayer videoDecoder;
   
        enum SyncMode {

			AUDIO_SYNCS_TO_VIDEO,
			VIDEO_SYNCS_TO_AUDIO

		};

        SyncMode syncMode;

		double previousVideoPts;
		double previousVideoDelay;

		double videoFrameTimer;
		double audioFrameTimer;

		HRTimer videoRefreshTimer;		
		HRTimer audioRefreshTimer;

		double videoPts;
		double videoPtsDrift;

		double audioDiffCum;
		double audioDiffAvgCoef;
		double audioDiffThreshold;
		int audioDiffAvgCount;

		bool seekRequest;
		double seekPosition;

        // direct3D stuff

        private ISceneHost Host;
        int nrVertices;
        int nrIndices;
        private InputLayout VertexLayout;
        private DataStream VertexStream;
        private Buffer Vertices;

        private DataStream IndexStream;
        private Buffer Indices;

        private Effect SimpleEffect;
        private Color4 OverlayColor = new Color4(1.0f);
        private Texture2D texture;
        
        GCHandle pinnedArray;
        ShaderResourceView textureView;

        Texture2D createTextureFromFile(string filename)
        {
            BitmapImage loadedImage = new BitmapImage();

            loadedImage.BeginInit();
            loadedImage.CacheOption = BitmapCacheOption.OnLoad;
            loadedImage.UriSource = new Uri(filename);
            loadedImage.EndInit();

            loadedImage.Freeze();

            int stride = loadedImage.PixelWidth * (loadedImage.Format.BitsPerPixel / 8);

            byte[] pixels = new byte[loadedImage.PixelHeight * stride];

            loadedImage.CopyPixels(pixels, stride, 0);

            pinnedArray = GCHandle.Alloc(pixels, GCHandleType.Pinned);
            IntPtr pixelPtr = pinnedArray.AddrOfPinnedObject();

            DataRectangle data = new DataRectangle(pixelPtr, stride);           

            var texDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.None,
                Format = Format.B8G8R8A8_UNorm,
                Height = loadedImage.PixelHeight,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Default,
                Width = loadedImage.PixelWidth
            };

            Texture2D texture = new Texture2D(Host.Device, texDesc, data);
                               
            return (texture);            
        }

        Texture2D createTexture(int width, int height)
        {
            var texDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = Format.B8G8R8A8_UNorm,
                Height = height,
                MipLevels = 1,
                OptionFlags = ResourceOptionFlags.None,
                SampleDescription = new SampleDescription(1, 0),
                Usage = ResourceUsage.Dynamic,
                Width = width
            };

            Texture2D texture = new Texture2D(Host.Device, texDesc);

            return (texture);
        }

        void IScene.Attach(ISceneHost host)
        {
            
            this.Host = host;

            Device device = host.Device;
            if (device == null)
                throw new Exception("Scene host device is null");

            try
            {

                Uri executablePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
                String shaderPath = System.IO.Path.GetDirectoryName(executablePath.LocalPath) + "\\Shaders\\";

                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile(shaderPath + "Simple.fx", "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);
                this.SimpleEffect = new Effect(device, shaderBytes);

            }
            catch (Exception e)
            {              
                System.Diagnostics.Debug.Print(e.Message);
                throw new Exception("Cannot compile shader code");
            }

            EffectTechnique technique = this.SimpleEffect.GetTechniqueByIndex(0); 
            EffectPass pass = technique.GetPassByIndex(0);

            this.VertexLayout = new InputLayout(device, pass.Description.Signature, new[] {
                new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),  
                new InputElement("TEXCOORD", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0)
                //new InputElement("COLOR", 0, Format.R32G32B32A32_Float, InputElement.AppendAligned, 0)  
               
            });

            int bytesPerVertexInfo = 4 * 8;          
            nrVertices = 4;

            this.VertexStream = new DataStream(bytesPerVertexInfo * nrVertices, true, true);
            this.VertexStream.WriteRange(new[] 
                {
                    new Vector4(-1.0f, 1.0f, 0.5f, 1.0f),
                    new Vector4(0.0f, 0.0f, 0.0f, 0.0f),

                    new Vector4(1.0f, -1.0f, 0.5f, 1.0f),
                    new Vector4(1.0f, 1.0f, 0.0f, 0.0f),

                    new Vector4(-1.0f, -1.0f, 0.5f, 1.0f),
                    new Vector4(0.0f, 1.0f, 0.0f, 0.0f),

                    new Vector4(1.0f, 1.0f, 0.5f, 1.0f),
                    new Vector4(1.0f, 0.0f, 0.0f, 0.0f)
                }
            );
            
            this.VertexStream.Position = 0;

            this.Vertices = new Buffer(device, this.VertexStream, new BufferDescription()
                {
                    BindFlags = BindFlags.VertexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = bytesPerVertexInfo * nrVertices,
                    Usage = ResourceUsage.Default
                }
            );

            nrIndices = 4;
            int indicesSizeBytes = nrIndices * sizeof(Int32);

            IndexStream = new DataStream(indicesSizeBytes, true, true);
            IndexStream.WriteRange<int>(new[] 
                {
                    3,1,0,2
                }
            );

            this.IndexStream.Position = 0;

            this.Indices = new Buffer(device, this.IndexStream, new BufferDescription()
                {
                    BindFlags = BindFlags.IndexBuffer,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None,
                    SizeInBytes = indicesSizeBytes,
                    Usage = ResourceUsage.Default
                }
            );

           initVideo();

            texture = createTexture(videoDecoder.Width, videoDecoder.Height);
           //texture = createTextureFromFile("d:\\dani.jpg");           
            
           ShaderResourceViewDescription desc = new ShaderResourceViewDescription();
           desc.Format = texture.Description.Format;
           desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
           desc.Texture2D.MipLevels = texture.Description.MipLevels;
           desc.Texture2D.MostDetailedMip = texture.Description.MipLevels - 1;
     
           textureView = new ShaderResourceView(device, texture, desc);
      
           device.Flush();

           
        }

        void initVideo()
        {
            videoDecoder = new VideoLib.VideoPlayer();
            videoDecoder.open("K:\\michelle jenneke\\barcelona.mp4", VideoLib.VideoPlayer.DecodedVideoFormat.BGRA);

            syncMode = SyncMode.AUDIO_SYNCS_TO_VIDEO;

            previousVideoPts = 0;
            previousVideoDelay = 0.04;

            audioDiffAvgCount = 0;

            videoRefreshTimer = HRTimerFactory.create(HRTimerFactory.TimerType.TIMER_QUEUE);
            videoRefreshTimer.Tick += new EventHandler(videoRefreshTimer_Tick);
            //videoRefreshTimer.SynchronizingObject = this;
            videoRefreshTimer.AutoReset = false;

            Task demuxPacketsTask = new Task(new Action(demuxPackets), TaskCreationOptions.LongRunning);
            demuxPacketsTask.Start();       

            Task updateAudioTask = new Task(new Action(updateAudio), TaskCreationOptions.LongRunning);
            updateAudioTask.Start();

            videoRefreshTimer.start();
        }

        void demuxPackets()
        {
            audioFrameTimer = videoFrameTimer = HRTimer.getTimestamp();

            bool success = true;

            do {

            } while(success = videoDecoder.demuxPacket());
        
        }

        void videoRefreshTimer_Tick(Object sender, EventArgs e)
        {

            bool skipVideoFrame = false;

restartvideo:
			
			double actualDelay = 0.04;


			// grab a decoded frame, returns false if the queue is stopped
			VideoFrame videoFrame = videoDecoder.FrameQueue.getDecodedVideoFrame();
/*
			if(VideoState == VideoState.CLOSED && videoFrame == null) {

				return;

			} else if(VideoState == VideoState.PLAYING) {
*/
				videoPts = videoFrame.Pts;
				videoPtsDrift = videoFrame.Pts + HRTimer.getTimestamp();

				if(skipVideoFrame == false) {

					videoFrame.copyFrameDataTexture(texture);
				} 					

				actualDelay = synchronizeVideo(videoPts);					
/*
			} else if(VideoState == VideoState.PAUSED) {

				videoRender.display(null, canvas, Color.Black, VideoRender.RenderMode.PAUSED);			
			}
*/
			// do not update ui elements on main thread inside videoStateLock
			// or we can get a deadlock
			//videoDebug.update();
			//updateUI();

			if(actualDelay < 0.010) {

				// delay is too small skip next frame
				skipVideoFrame = true;
				//videoDebug.NrVideoFramesDropped = videoDebug.NrVideoFramesDropped + 1;
				goto restartvideo;

			} 

			// start timer with delay for next frame
			videoRefreshTimer.Interval = (int)(actualDelay * 1000 + 0.5);
			videoRefreshTimer.start();		
            
        }

        double synchronizeVideo(double videoPts)
        {

            // assume delay to next frame equals delay between previous frames
            double delay = videoPts - previousVideoPts;

            if (delay <= 0 || delay >= 1.0)
            {
                // if incorrect delay, use previous one 
                delay = previousVideoDelay;
            }

            previousVideoPts = videoPts;
            previousVideoDelay = delay;

            if (videoDecoder.HasAudio && syncMode == SyncMode.VIDEO_SYNCS_TO_AUDIO)
            {
                /*
                    // synchronize video to audio
                    double diff = getVideoClock() - audioPlayer.getAudioClock();

                    // Skip or repeat the frame. Take delay into account
                    // FFPlay still doesn't "know if this is the best guess."
                    double sync_threshold = (delay > AV_SYNC_THRESHOLD) ? delay : AV_SYNC_THRESHOLD;

                    if(Math::Abs(diff) < AV_NOSYNC_THRESHOLD) {

                        if(diff <= -sync_threshold) {

                            delay = 0;

                        } else if(diff >= sync_threshold) {

                            delay = 2 * delay;
                        }
                    }
                */
            }
            // adjust delay based on the actual current time
            videoFrameTimer += delay;
            double actualDelay = videoFrameTimer - HRTimer.getTimestamp();
/*
            videoDebug.VideoDelay = delay;
            videoDebug.ActualVideoDelay = actualDelay;
            videoDebug.VideoSync = getVideoClock();
            videoDebug.AudioSync = audioPlayer.getAudioClock();
            videoDebug.VideoQueueSize = videoDecoder.FrameQueue.VideoPacketsInQueue;
            videoDebug.AudioQueueSize = videoDecoder.FrameQueue.AudioPacketsInQueue;
*/

            return (actualDelay);
        }

        void updateAudio()
        {
             while (true)
            {
             VideoLib.AudioFrame audioFrame = videoDecoder.FrameQueue.getDecodedAudioFrame();
            }

        }


        void IScene.Detach()
        {
            Disposer.RemoveAndDispose(ref this.Vertices);
            Disposer.RemoveAndDispose(ref this.VertexLayout);
            Disposer.RemoveAndDispose(ref this.SimpleEffect);
            Disposer.RemoveAndDispose(ref this.VertexStream);

            Disposer.RemoveAndDispose(ref this.Indices);
            Disposer.RemoveAndDispose(ref this.IndexStream);

            Disposer.RemoveAndDispose(ref this.texture);
            Disposer.RemoveAndDispose(ref this.textureView);
            pinnedArray.Free();
        }

        void IScene.Update(TimeSpan sceneTime)
        {
           

        }

        void IScene.Render()
        {
            Device device = this.Host.Device;
            if (device == null)
                return;          

            device.InputAssembler.InputLayout = this.VertexLayout;
            device.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip;
            device.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.Vertices, 32, 0));
            device.InputAssembler.SetIndexBuffer(Indices, Format.R32_UInt, 0);
                       
            EffectTechnique technique = this.SimpleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);
          
            //EffectVectorVariable overlayColor = this.SimpleEffect.GetVariableBySemantic("OverlayColor").AsVector();
            //overlayColor.Set(this.OverlayColor);          
            
            for (int i = 0; i < technique.Description.PassCount; ++i)
            {
                pass.Apply();

                device.PixelShader.SetShaderResource(0, textureView);  

                device.DrawIndexed(nrIndices, 0, 0);
            }
        }
    }
}
