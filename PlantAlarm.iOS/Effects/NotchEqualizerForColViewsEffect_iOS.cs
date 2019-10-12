using System;
using PlantAlarm.iOS.Effects;
using PclEffect = PlantAlarm.Effects.NotchEqualizerForColViewsEffect;
using PclSafeAreaEffect = PlantAlarm.Effects.SafeAreaEffect;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using System.Linq;
using Xamarin.Forms.Xaml;

[assembly: ExportEffect(typeof(NotchEqualizerForColViewsEffect), nameof(NotchEqualizerForColViewsEffect))]
namespace PlantAlarm.iOS.Effects
{
    public class NotchEqualizerForColViewsEffect : PlatformEffect
    {
        protected override void OnAttached()
        {
            if (Element is Layout element)
            {
                var rootElement = Element;
                while (rootElement.Parent != null && rootElement.Parent is Layout) rootElement = rootElement.Parent;

                var safeAreaEffect = (PclSafeAreaEffect)rootElement.Effects.FirstOrDefault(eff => eff is PclSafeAreaEffect);

                var effect = (PclEffect)element.Effects.First(eff => eff is PclEffect);
                var insets = UIApplication.SharedApplication.Windows[0].SafeAreaInsets; // Can't use KeyWindow this early
                if (insets.Top > (safeAreaEffect != null ? 20 : 0)) // We have a notch (20 was added in SafeAreaEff)
                {
                    element.HeightRequest = effect.NotchHeightRequest;
                }
            }
        }

        protected override void OnDetached()
        {
        }
    }
}
