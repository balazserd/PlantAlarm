using System;
using System.Linq;
using Xamarin.Forms;

namespace PlantAlarm.Effects
{
    public class BorderEffect : RoutingEffect
    {
        public float Thickness { get; set; }

        public Color Color { get; set; }

        public void SetColorViaView(View v, Color color)
        {
            v.Effects.Remove(this);
            this.Color = color;
            v.Effects.Add(this);
        }
        
        public BorderEffect() : base($"EBUniApps.{nameof(BorderEffect)}")
        {
            Color = Color.Black;
        }
    }

    public static class StBorderEffect
    {
        public static BindableProperty ColorProperty = BindableProperty.Create(
            "Color",
            typeof(Color),
            typeof(BorderEffect),
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                View boundControl = bindable as View;
                BorderEffect effect = boundControl.Effects.First(eff => eff is BorderEffect) as BorderEffect;

                effect.SetColorViaView(boundControl, (Color)newValue);
            });
    }
}
