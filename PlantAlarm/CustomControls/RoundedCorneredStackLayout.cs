using System;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public class RoundedCorneredStackLayout : StackLayout
    {
        private bool isSelected { get; set; }
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                OnPropertyChanged();
            }
        }

        public static readonly BindableProperty IsSelectedProperty = BindableProperty.Create(
            nameof(IsSelected),
            typeof(bool),
            typeof(RoundedCorneredStackLayout),
            false,
            propertyChanged: (bindable, oldValue, newValue) =>
            {
                var sl = bindable as RoundedCorneredStackLayout;
                sl.IsSelected = (bool)newValue;
            });

        public RoundedCorneredStackLayout()
        {
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            this.HeightRequest = width;
            base.OnSizeAllocated(width, height);
        }
    }
}
