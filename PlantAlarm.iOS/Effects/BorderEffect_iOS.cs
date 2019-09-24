using System;
using PlantAlarm.iOS.Effects;
using UIKit;
using PclBorderEffect = PlantAlarm.Effects.BorderEffect;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Linq;

[assembly: ExportEffect(typeof(BorderEffect), nameof(BorderEffect))]
namespace PlantAlarm.iOS.Effects
{
    public class BorderEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var effect = (PclBorderEffect)Element.Effects.FirstOrDefault(e => e is PclBorderEffect);
            Control.Layer.BorderWidth = effect.Thickness;
            Control.Layer.BorderColor = UIColor.Black.CGColor;
        }

        protected override void OnDetached()
        {
        }
    }
}
