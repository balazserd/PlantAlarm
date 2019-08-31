using System;
using Xamarin.Forms;

namespace PlantAlarm.Behaviors
{
    /// <summary>
    /// This behavior checks if the entry contains a number between 1 and 99, inclusive.
    /// </summary>
    public class DoubleDigitEntryCheckingBehavior : Behavior<Entry>
    {
        protected override void OnAttachedTo(Entry bindable)
        {
            bindable.TextChanged += CheckIfValueIsOkay;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(Entry bindable)
        {
            bindable.TextChanged -= CheckIfValueIsOkay;
            base.OnDetachingFrom(bindable);
        }

        private void CheckIfValueIsOkay(object sender, TextChangedEventArgs e)
        {
            bool isNotNumber = !int.TryParse(e.NewTextValue, out _);

            if (isNotNumber && e.NewTextValue.Length > 0)
            {
                (sender as Entry).Text = e.OldTextValue ?? string.Empty;
            }
        }
    }
}
