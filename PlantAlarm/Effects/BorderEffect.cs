using System;
using Xamarin.Forms;

namespace PlantAlarm.Effects
{
    public class BorderEffect : RoutingEffect
    {
        public float Thickness { get; set; }
        public BorderEffect() : base($"EBUniApps.{nameof(BorderEffect)}")
        {
        }
    }
}
