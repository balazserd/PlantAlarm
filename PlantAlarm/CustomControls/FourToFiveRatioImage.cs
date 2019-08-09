using System;
using Xamarin.Forms;

namespace PlantAlarm.CustomControls
{
    public class FourToFiveRatioImage : Image
    {
        protected override void OnSizeAllocated(double width, double height)
        {
            this.HeightRequest = width * 0.8;
            base.OnSizeAllocated(width, height);
        }
    }
}
