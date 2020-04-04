using System;
using System.ComponentModel;
using Xamarin.Forms;

namespace PlantAlarm.Behaviors
{
    public class CapitalizingBehavior : Behavior<Label>
    {
        protected override void OnAttachedTo(Label bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.Text = bindable.Text.ToUpper();
            bindable.PropertyChanged += this.MakeUpperCase;
        }

        protected override void OnDetachingFrom(Label bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.PropertyChanged -= this.MakeUpperCase;
        }

        private void MakeUpperCase(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "Text")
            {
                var label = sender as Label;
                label.Text = label.Text.ToUpper();
            }
        }
    }
}
