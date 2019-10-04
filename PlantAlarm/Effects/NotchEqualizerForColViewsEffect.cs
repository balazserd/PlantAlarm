using System;
using Xamarin.Forms;

namespace PlantAlarm.Effects
{
    public class NotchEqualizerForColViewsEffect : RoutingEffect
    {
        public double NotchHeightRequest { get; set; }

        public NotchEqualizerForColViewsEffect() : base($"EBUniApps.{nameof(NotchEqualizerForColViewsEffect)}")
        {
        }
    }
}
