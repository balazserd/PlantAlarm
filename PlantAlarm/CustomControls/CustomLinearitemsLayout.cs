using System;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public class CustomLinearItemsLayout : ItemsLayout
    {
        public CustomLinearItemsLayout([CustomParameter("Orientation")] ItemsLayoutOrientation orientation) : base(orientation)
        {
        }

        public static readonly IItemsLayout Vertical = new CustomLinearItemsLayout(ItemsLayoutOrientation.Vertical);
        public static readonly IItemsLayout Horizontal = new CustomLinearItemsLayout(ItemsLayoutOrientation.Horizontal);

        internal static readonly CustomLinearItemsLayout CarouselDefault = new CustomLinearItemsLayout(ItemsLayoutOrientation.Horizontal)
        {
            SnapPointsAlignment = SnapPointsAlignment.Center,
            SnapPointsType = SnapPointsType.Mandatory
        };

        public static readonly BindableProperty ItemSpacingProperty =
            BindableProperty.Create(nameof(ItemSpacing), typeof(double), typeof(CustomLinearItemsLayout), default(double),
                validateValue: (bindable, value) =>
                {
                    try
                    {
                        Convert.ToDouble(value);
                        return true;
                    }
                    catch (Exception x)
                    {
                        if (x is InvalidCastException || x is OverflowException || x is FormatException)
                        {
                            return false;
                        }
                        throw x;
                    }
                });

        public double ItemSpacing
        {
            get => (double)GetValue(ItemSpacingProperty);
            set => SetValue(ItemSpacingProperty, value);
        }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    internal sealed class CustomParameterAttribute : Attribute
    {
        public CustomParameterAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}