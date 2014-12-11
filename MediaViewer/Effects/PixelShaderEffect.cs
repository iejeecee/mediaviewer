using MediaViewer.Infrastructure.Logging;
using SharpDX.D3DCompiler;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace MediaViewer.Effects
{
    class PixelShaderEffect : ShaderEffect
    {
        

        protected PixelShaderEffect()
        {

        }

        protected void compileAndSaveShader(string name, string entryPoint)
        {
            Uri executablePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            String shaderPath = System.IO.Path.GetDirectoryName(executablePath.LocalPath) + "\\Shaders\\";

            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile(shaderPath + name, entryPoint, "ps_2_0", ShaderFlags.None, EffectFlags.None, null, null);
                Logger.Log.Info("Compiled pixel shader: " + shaderPath + name);
                FileStream stream = new FileStream("D:\\Repos\\mediaviewer\\MediaViewer\\Resources\\CompiledShaders\\" + System.IO.Path.GetFileNameWithoutExtension(name) + ".ps", FileMode.Create);
                shaderBytes.Save(stream);
                stream.Close();                             
            }
            catch (Exception e)
            {
                Logger.Log.Error("Error loading pixel shader " + name, e);
                throw;
            }
        }

        protected void loadShader(String name)
        {
            PixelShader = new PixelShader();
       
            PixelShader.UriSource = new Uri("pack://application:,,,/MediaViewer;component/Resources/CompiledShaders/" + name);

        }
        
        protected void compileAndLoadShader(String name, String entryPoint) {

            PixelShader = new PixelShader();
            Uri executablePath = new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            String shaderPath = System.IO.Path.GetDirectoryName(executablePath.LocalPath) + "\\Shaders\\";

            try
            {
                ShaderBytecode shaderBytes = ShaderBytecode.CompileFromFile(shaderPath + name, entryPoint, "ps_2_0", ShaderFlags.None, EffectFlags.None, null, null);
                Logger.Log.Info("Compiled pixel shader: " + shaderPath + name);
                MemoryStream stream = new MemoryStream();
                shaderBytes.Save(stream);
                stream.Position = 0;

                PixelShader.SetStreamSource(stream);                
                
            }
            catch (Exception e)
            {
                Logger.Log.Error("Error loading pixel shader " + name, e);
                throw;
            }
        }

        
    }
}
