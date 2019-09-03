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
            Control.Layer.CornerRadius = effect.Radius;
        }

        protected override void OnDetached()
        {
        }
    }
}
