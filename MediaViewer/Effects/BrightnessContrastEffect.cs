using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Effects;

namespace MediaViewer.Effects
{
    class BrightnessContrastEffect : PixelShaderEffect
    {
        public BrightnessContrastEffect()
        {
            //saveShader("BrightnessContrast.fx", "MainPS");
            loadShader("BrightnessContrast.ps");
            UpdateShaderValue(InputProperty);
           
        }

        public double Brightness
        {
            get { return (double)GetValue(BrightnessProperty); }
            set { SetValue(BrightnessProperty, value); }
        }

        public static readonly DependencyProperty BrightnessProperty =
            DependencyProperty.Register("Brightness", typeof(double), typeof(BrightnessContrastEffect),
              new UIPropertyMetadata(0.0d, PixelShaderConstantCallback(0)));

        public double Contrast
        {
            get { return (double)GetValue(ContrastProperty); }
            set { SetValue(ContrastProperty, value); }
        }

        public static readonly DependencyProperty ContrastProperty =
            DependencyProperty.Register("Contrast", typeof(double), typeof(BrightnessContrastEffect),
              new UIPropertyMetadata(0.99d, PixelShaderConstantCallback(1)));

        public static readonly DependencyProperty InputProperty =
            ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(BrightnessContrastEffect), 0);

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }
    }
}
