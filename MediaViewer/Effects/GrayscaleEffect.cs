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

    class GrayscaleEffect : PixelShaderEffect
    {
        public GrayscaleEffect()
        {
            //compileAndLoadShader("Grayscale.fx", "MainPS");
            //saveShader("Grayscale.fx", "MainPS");
            loadShader("Grayscale.ps");
            UpdateShaderValue(InputProperty);
        }

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(GrayscaleEffect), 0);

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }
    }

}
