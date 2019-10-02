using System;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public class ShadowLayout : StackLayout
    {
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public float ShadowRadius { get; set; }
        public float ShadowOpacity { get; set; }

        public ShadowLayout()
        {
        }
    }
}
