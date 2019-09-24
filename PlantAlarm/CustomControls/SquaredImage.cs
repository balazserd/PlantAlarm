using System;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public class SquaredImage : Image
    {
        public SquaredImage()
        {
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            this.HeightRequest = width;
            base.OnSizeAllocated(width, width);
        }
    }
}
