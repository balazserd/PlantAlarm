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
            UIView view = this.Control ?? this.Container;

            var effect = (PclRoundedEffect)Element.Effects.FirstOrDefault(e => e is PclRoundedEffect);
            view.Layer.CornerRadius = Math.Abs(effect.Radius) > 0.01 ? effect.Radius : view.Layer.Frame.Width / 2;
            view.Layer.MasksToBounds = false;
            view.ClipsToBounds = true;
        }

        protected override void OnDetached()
        {
        }
    }
}
