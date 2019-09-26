using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace PlantAlarm.Behaviors
{
    /// <summary>
    /// When your VisualElement's IsEnabled property becomes true, it will fade in.
    /// When your VisualElement's IsEnabled property becomes false, it will fade out.
    /// </summary>
    public class FadingBehavior : Behavior<VisualElement>
    {
        public uint FadeTime { get; set; } = 250;

        protected override void OnAttachedTo(VisualElement bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.PropertyChanged += VisibilityMightHaveChanged;
        }

        protected override void OnDetachingFrom(VisualElement bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.PropertyChanged -= VisibilityMightHaveChanged;
        }

        private void VisibilityMightHaveChanged(object sender, PropertyChangedEventArgs args)
        {
            var element = sender as VisualElement;
            
            if (args.PropertyName == VisualElement.IsVisibleProperty.PropertyName)
            {
                var fadeToValue = element.IsVisible ? 1 : 0;
                element.Opacity = -fadeToValue + 1;
                element.FadeTo(fadeToValue, FadeTime);
            }
        }
    }
}
