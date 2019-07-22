using System;
using PlantAlarm.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ResolutionGroupName("EBUniApps")]
[assembly: ExportEffect(typeof(BorderlessEffect), nameof(BorderlessEffect))]
namespace PlantAlarm.iOS.Effects
{
    public class BorderlessEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Control is UITextField TextControl)
            {
                TextControl.Layer.BorderWidth = 0;
                TextControl.BorderStyle = UITextBorderStyle.None;
            }
            else
            {
                throw new InvalidCastException("The Effect can only be used on UITextField objects.");
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
