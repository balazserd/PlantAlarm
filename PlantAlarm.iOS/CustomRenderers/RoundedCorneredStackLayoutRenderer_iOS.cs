using System;
using CoreGraphics;
using PlantAlarm.iOS.CustomRenderers;
using PlantAlarm.CustomControls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(RoundedCorneredStackLayout), typeof(RoundedCorneredStackLayoutRenderer_iOS))]
namespace PlantAlarm.iOS.CustomRenderers
{
    public class RoundedCorneredStackLayoutRenderer_iOS : VisualElementRenderer<RoundedCorneredStackLayout>
    {
        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RoundedCorneredStackLayout.IsSelected))
            {
                setSelected();
            }

            base.OnElementPropertyChanged(sender, e);
        }

        public RoundedCorneredStackLayoutRenderer_iOS()
        {
            Layer.CornerRadius = 10;

            setSelected();

            Layer.ShadowOffset = new CGSize(3, 3);
            Layer.ShadowOpacity = 0.4f;
            Layer.ShadowRadius = 3;
        }

        private void setSelected()
        {
            bool isSelected = this.Element?.IsSelected ?? false;

            Layer.BorderWidth = isSelected ? 2 : 1;
            Layer.BorderColor = isSelected ? UIColor.FromRGB(255, 215, 0).CGColor : UIColor.FromRGB(166, 171, 171).CGColor;
            Layer.BackgroundColor = isSelected ? UIColor.FromRGB(255, 247, 204).CGColor : UIColor.FromRGB(246, 246, 246).CGColor;
        }
    }
}
