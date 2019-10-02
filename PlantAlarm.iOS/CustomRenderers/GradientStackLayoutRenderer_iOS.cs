using System;
using System.ComponentModel;
using System.Linq;
using CoreAnimation;
using CoreGraphics;
using PlantAlarm.CustomControls;
using PlantAlarm.iOS.CustomRenderers;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(GradientStackLayout), typeof(GradientStackLayoutRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class GradientStackLayoutRenderer_iOS : VisualElementRenderer<GradientStackLayout>
    {
        private bool HasBeenDrawnBefore = false;

        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            var element = (Element as GradientStackLayout);

            var colors = new CGColor[] {
                element.GradientFromColor.ToCGColor(),
                element.GradientToColor.ToCGColor()
            };

            var gradientLayer = new CAGradientLayer();

            switch (element.GradientDirection)
            {
                default:
                case GradientStyle.ToRight:
                    gradientLayer.StartPoint = new CGPoint(0, 0.5);
                    gradientLayer.EndPoint = new CGPoint(1, 0.5);
                    break;
                case GradientStyle.ToLeft:
                    gradientLayer.StartPoint = new CGPoint(1, 0.5);
                    gradientLayer.EndPoint = new CGPoint(0, 0.5);
                    break;
                case GradientStyle.ToTop:
                    gradientLayer.StartPoint = new CGPoint(0.5, 0);
                    gradientLayer.EndPoint = new CGPoint(0.5, 1);
                    break;
                case GradientStyle.ToBottom:
                    gradientLayer.StartPoint = new CGPoint(0.5, 1);
                    gradientLayer.EndPoint = new CGPoint(0.5, 0);
                    break;
                case GradientStyle.ToTopLeft:
                    gradientLayer.StartPoint = new CGPoint(1, 0);
                    gradientLayer.EndPoint = new CGPoint(0, 1);
                    break;
                case GradientStyle.ToTopRight:
                    gradientLayer.StartPoint = new CGPoint(0, 1);
                    gradientLayer.EndPoint = new CGPoint(1, 0);
                    break;
                case GradientStyle.ToBottomLeft:
                    gradientLayer.StartPoint = new CGPoint(1, 1);
                    gradientLayer.EndPoint = new CGPoint(0, 0);
                    break;
                case GradientStyle.ToBottomRight:
                    gradientLayer.StartPoint = new CGPoint(0, 0);
                    gradientLayer.EndPoint = new CGPoint(1, 1);
                    break;
            }

            gradientLayer.Frame = rect;
            gradientLayer.Colors = colors;

            int gradientIndex = GetHighestGradientLayerIndex();

            NativeView?.Layer?.InsertSublayer(gradientLayer, gradientIndex);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);
            SizeToFit();
            var frame = NativeView.Frame;
            Draw(new CGRect(frame.X, frame.Y, frame.Width, frame.Height));
        }

        private int GetHighestGradientLayerIndex()
        {
            if (NativeView?.Layer?.Sublayers == null) return -1;

            int i;
            for (i = 0; i < NativeView.Layer.Sublayers.Count(); i++)
            {
                if (!(NativeView.Layer.Sublayers[i] is CAGradientLayer)) return i;
            }

            return i;
        }
    }
}
