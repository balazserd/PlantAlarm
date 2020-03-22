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

        public void SetThicknessViaView(View v, float thickness)
        {
            v.Effects.Remove(this);
            this.Thickness = thickness;
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

        public static BindableProperty ThicknessProperty = BindableProperty.CreateAttached(
            "Thickness",
            typeof(float),
            typeof(BorderEffect),
            0.0f,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                View boundControl = bindable as View;
                BorderEffect effect = boundControl.Effects.FirstOrDefault(eff => eff is BorderEffect) as BorderEffect;

                if (effect != null)
                {
                    boundControl.Effects.Remove(effect);
                }
                boundControl.Effects.Add(new BorderEffect()
                {
                    Thickness = (float)newValue,
                    Color = effect.Color
                });
            });
        public static float GetThickness(BindableObject element) => (float)element.GetValue(ThicknessProperty);
        public static void SetThickness(BindableObject element, float value) => element.SetValue(ThicknessProperty, value);
    }
}
