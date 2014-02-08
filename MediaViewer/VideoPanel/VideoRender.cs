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
    using System.Windows;


    public class VideoRender : IScene
    {
    
        private ISceneHost Host;
        int nrVertices;
        int nrIndices;
        private InputLayout VertexLayout;
        private DataStream VertexStream;
        private Buffer Vertices;

        private DataStream IndexStream;
        private Buffer Indices;

        private Effect SimpleEffect;

        private int nrTextures;
        private Texture2D[] yuvTexture;
  
        Viewport viewport;
        
        GCHandle pinnedArray;
        ShaderResourceView[] textureView;

        VideoPlayerViewModel videoPlayerViewModel;

        public VideoPlayerViewModel VideoPlayerViewModel
        {
            get { return videoPlayerViewModel; }          
        }

        public VideoRender()
        {
            //videoPlayerViewModel = new VideoPlayerViewModel(displayVideoFrame, VideoLib.VideoPlayer.DecodedVideoFormat.BGRA);
            videoPlayerViewModel = new VideoPlayerViewModel(displayVideoFrame, VideoLib.VideoPlayer.DecodedVideoFormat.YUV420P);
            videoPlayerViewModel.VideoOpened += new EventHandler(videoPanelViewModel_VideoOpened);

            yuvTexture = new Texture2D[3] { null, null, null };
            textureView = new ShaderResourceView[3] { null, null, null };
            nrTextures = 0;
        }

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

        Texture2D createTexture(int width, int height, Format format)
        {
            var texDesc = new Texture2DDescription
            {
                ArraySize = 1,
                BindFlags = BindFlags.ShaderResource,
                CpuAccessFlags = CpuAccessFlags.Write,
                Format = format,
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

            Uri executablePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            String shaderPath = System.IO.Path.GetDirectoryName(executablePath.LocalPath) + "\\Shaders\\";

            if (videoPlayerViewModel.DecodedVideoFormat == VideoLib.VideoPlayer.DecodedVideoFormat.YUV420P)
            {
                shaderPath += "YUVtoRGB.fx";
            }
            else
            {
                shaderPath += "Simple.fx";
            }

            try
            {            
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile(shaderPath, "fx_4_0", ShaderFlags.None, EffectFlags.None, null, null);             
                this.SimpleEffect = new Effect(device, shaderBytes);             
            }
            catch (Exception e)
            {              
                System.Diagnostics.Debug.Print(e.Message);
                throw new Exception("Error compiling: " + shaderPath);
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
  
            device.Flush();         
        }

        void videoPanelViewModel_VideoOpened(Object sender, EventArgs e)
        {

            for (int i = 0; i < nrTextures; i++)
            {
                Disposer.RemoveAndDispose(ref this.yuvTexture[i]);
                Disposer.RemoveAndDispose(ref this.textureView[i]);
            }
                                
            if (videoPlayerViewModel.DecodedVideoFormat == VideoLib.VideoPlayer.DecodedVideoFormat.YUV420P)
            {               
                int width = videoPlayerViewModel.Width;
                int height = videoPlayerViewModel.Height;

                yuvTexture[0] = createTexture(width, height, Format.R8_UNorm);
                yuvTexture[1] = createTexture(width / 2, height / 2, Format.R8_UNorm);
                yuvTexture[2] = createTexture(width / 2, height / 2, Format.R8_UNorm);

                nrTextures = 3;
            }
            else
            {                
                yuvTexture[0] = createTexture(videoPlayerViewModel.Width, videoPlayerViewModel.Height, Format.B8G8R8A8_UNorm);

                nrTextures = 1;
            }

            Device device = Host.Device;

            for (int i = 0; i < nrTextures; i++)
            {
                ShaderResourceViewDescription desc = new ShaderResourceViewDescription();
                desc.Format = yuvTexture[i].Description.Format;
                desc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.Texture2D;
                desc.Texture2D.MipLevels = yuvTexture[i].Description.MipLevels;
                desc.Texture2D.MostDetailedMip = yuvTexture[i].Description.MipLevels - 1;

                textureView[i] = new ShaderResourceView(device, yuvTexture[i], desc);

            }
  
            viewport = setupViewport(videoPlayerViewModel.Width, videoPlayerViewModel.Height);
           
            device.Flush();

            DPFCanvas canvas = (DPFCanvas)Host;
            canvas.StartRendering();
             
        }

        Viewport setupViewport(int videoWidth, int videoHeight)
        {
            DPFCanvas canvas = (DPFCanvas)Host;
            double screenWidth = canvas.ActualWidth;
            double screenHeight = canvas.ActualHeight;

            System.Drawing.Rectangle screenRect = new System.Drawing.Rectangle(0,0,(int)screenWidth, (int)screenHeight);

            System.Drawing.Rectangle videoRect = Utils.ImageUtils.stretchRectangle(new System.Drawing.Rectangle(0,0,videoWidth,videoHeight),
                screenRect);

            videoRect = Utils.ImageUtils.centerRectangle(screenRect, videoRect);

            Viewport viewport = new Viewport(new DrawingRectangle(videoRect.X, videoRect.Y, videoRect.Width, videoRect.Height));

            return (viewport);
        }

        void IScene.RenderSizeChanged(SizeChangedInfo sizeChange)
        {
            viewport = setupViewport(videoPlayerViewModel.Width, videoPlayerViewModel.Height);
        }

        void displayVideoFrame(VideoLib.VideoFrame videoFrame)
        {
            videoFrame.copyFrameDataTexture(yuvTexture);
        }
        
        void IScene.Detach()
        {
            Disposer.RemoveAndDispose(ref this.Vertices);
            Disposer.RemoveAndDispose(ref this.VertexLayout);
            Disposer.RemoveAndDispose(ref this.SimpleEffect);
            Disposer.RemoveAndDispose(ref this.VertexStream);

            Disposer.RemoveAndDispose(ref this.Indices);
            Disposer.RemoveAndDispose(ref this.IndexStream);

            for (int i = 0; i < nrTextures; i++)
            {
                Disposer.RemoveAndDispose(ref this.yuvTexture[i]);
                Disposer.RemoveAndDispose(ref this.textureView[i]);
            }

            if (pinnedArray.IsAllocated)
            {
                pinnedArray.Free();
            }
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

            device.Rasterizer.SetViewports(viewport);

            EffectTechnique technique = this.SimpleEffect.GetTechniqueByIndex(0);
            EffectPass pass = technique.GetPassByIndex(0);
          
            //EffectVectorVariable overlayColor = this.SimpleEffect.GetVariableBySemantic("OverlayColor").AsVector();
            //overlayColor.Set(this.OverlayColor);          
            
            for (int i = 0; i < technique.Description.PassCount; ++i)
            {
                pass.Apply();

                for (int j = 0; j < nrTextures; j++)
                {
                    device.PixelShader.SetShaderResource(j, textureView[j]);
                }

                device.DrawIndexed(nrIndices, 0, 0);
            }
        }
    }
}
