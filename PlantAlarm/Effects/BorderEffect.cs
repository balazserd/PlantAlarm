using System;
using Xamarin.Forms;

namespace PlantAlarm.Effects
{
    public class BorderEffect : RoutingEffect
    {
        public float Thickness { get; set; }
        public Color Color { get; set; }

        public BorderEffect() : base($"EBUniApps.{nameof(BorderEffect)}")
        {
            Color = Color.Black;
        }
    }
}
