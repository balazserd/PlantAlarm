using System;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public class GradientStackLayout : StackLayout
    {
        public Color GradientFromColor { get; set; }
        public Color GradientToColor { get; set; }

        public GradientStyle GradientDirection { get; set; }
    }

    public enum GradientStyle
    {
        ToRight = 1,
        ToLeft,
        ToTop,
        ToBottom,
        ToTopLeft,
        ToTopRight,
        ToBottomLeft,
        ToBottomRight
    }
}
