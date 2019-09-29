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
            UIView view = this.Control ?? this.Container;

            var effect = (PclBorderEffect)Element.Effects.FirstOrDefault(e => e is PclBorderEffect);
            view.Layer.BorderWidth = effect.Thickness;
            view.Layer.BorderColor = effect.Color.ToCGColor();
        }

        protected override void OnDetached()
        {
        }
    }
}
