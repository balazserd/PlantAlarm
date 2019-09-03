using System;
using Xamarin.Forms;

namespace PlantAlarm.Effects
{
    public class RoundedCornerEffect : RoutingEffect
    {
        public float Radius { get; set; }
        public RoundedCornerEffect() : base($"EBUniApps.{nameof(RoundedCornerEffect)}")
        {
        }
    }
}
