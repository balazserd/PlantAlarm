using System;
using CoreGraphics;
using PlantAlarm.CustomControls;
using PlantAlarm.iOS.CustomRenderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ShadowLayout), typeof(ShadowLayoutRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class ShadowLayoutRenderer_iOS : VisualElementRenderer<ShadowLayout>
    {
        public override void Draw(CGRect rect)
        {
            base.Draw(rect);
            NativeView.Layer.ShadowOffset = new CGSize(Element.OffsetX, Element.OffsetY);
            NativeView.Layer.ShadowOpacity = Element.ShadowOpacity;
            NativeView.Layer.ShadowRadius = Element.ShadowRadius;
            NativeView.Layer.ShadowColor = UIColor.Black.CGColor;
        }
    }
}
