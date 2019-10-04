using System;
using PlantAlarm.iOS.Effects;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

//This file was created based on this article: https://xamarinhelp.com/safeareainsets-xamarin-forms-ios/

[assembly: ExportEffect(typeof(SafeAreaEffect), nameof(SafeAreaEffect))]
namespace PlantAlarm.iOS.Effects
{
    public class SafeAreaEffect : PlatformEffect
    {
        Thickness _padding;
        protected override void OnAttached()
        {
            if (Element is Layout element)
            {
                _padding = element.Padding;
                var insets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets; // Can't use KeyWindow this early
                if (insets.Top > 0) // We have a notch
                {
                    element.Padding = new Thickness(_padding.Left + insets.Left, _padding.Top + insets.Top, _padding.Right + insets.Right, _padding.Bottom + insets.Bottom);
                    return;
                }
            }
        }

        protected override void OnDetached()
        {
            //if (Element is Layout element)
            //{
            //    element.Padding = _padding;
            //}
        }
    }
}
