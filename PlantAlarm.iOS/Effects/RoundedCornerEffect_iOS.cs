using System;
using System.Linq;
using PlantAlarm.iOS.Effects;
using PclRoundedEffect = PlantAlarm.Effects.RoundedCornerEffect;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportEffect(typeof(RoundedCornerEffect), nameof(RoundedCornerEffect))]
namespace PlantAlarm.iOS.Effects
{
    public class RoundedCornerEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            var effect = (PclRoundedEffect)Element.Effects.FirstOrDefault(e => e is PclRoundedEffect);
            Control.Layer.CornerRadius = Math.Abs(effect.Radius) > 0.01 ? effect.Radius : Control.Layer.Frame.Width / 2;
            Control.Layer.MasksToBounds = false;
            Control.ClipsToBounds = true;
        }

        protected override void OnDetached()
        {
        }
    }
}
